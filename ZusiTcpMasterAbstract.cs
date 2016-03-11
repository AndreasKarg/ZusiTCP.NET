#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Zusi_Datenausgabe
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class ZusiTcpMasterAbstract : ZusiTcpBaseConnection
  {
    private HashSet<int> _requestedData = null;

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpMasterAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    /// <param name="priority">If tis class is used as a server-based client you can use an alternate priority here.</param>
    protected ZusiTcpMasterAbstract(string clientId, SynchronizationContext hostContext, ClientPriority priority)
      : base(clientId, priority, hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpMasterAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    protected ZusiTcpMasterAbstract(string clientId, SynchronizationContext hostContext)
      : base(clientId, ClientPriority.Master, hostContext)
    {
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpMasterAbstract" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    protected ZusiTcpMasterAbstract(string clientId)
      : base(clientId, ClientPriority.Master)
    {
    }

    public HashSet<int> RequestedData
    {
      [DebuggerStepThrough]
      get
      {
        Debug.Assert(_requestedData != null);
        return _requestedData;
      }
    }

    public event EventHandler<EventArgs> DataRequested;
    ///<summary>Allows the Server to check if the Data Request can be terminated early. 
    /// If someone throws an exception in this Event, data will be refused.</summary>
    public event EventHandler<EventArgs> DataChecking;

    private void OnDataRequested(ICollection<int> requestedData)
    {
      if (DataRequested == null) return;
      DataRequested.Invoke(this, EventArgs.Empty);
    }
    private void OnDataChecking(ICollection<int> requestedData)
    {
      if (DataChecking == null) return;
      DataChecking.Invoke(this, EventArgs.Empty);
    }

    protected override void HandleHandshake()
    {
      PacketSender.SendLargePacket(
        PacketSender.Pack(0, 1, 1, (byte) ClientPriority, Convert.ToByte(StringEncoder.GetByteCount(ClientId))),
        StringEncoder.GetBytes(ClientId)); //protocol-Version 1

      ExpectResponse(ResponseType.AckHello, 0);

      ExpectRequestData();
    }

    protected void ExpectRequestData()
    {
      ExpectResponseAnswer requestedValues = null;
      const int dataGroup = -1;
      while ((requestedValues == null) || (requestedValues.RequestedDataGroup != 0) ||
             ((requestedValues.RequestedValues != null) && (requestedValues.RequestedValues.Length != 0)))
      {
        try
        {
          requestedValues = ExpectResponse(ResponseType.NeededData, dataGroup);
        }
        catch
        {
          PacketSender.SendPacket(PacketSender.Pack(0, 4, 255));
          throw;
        }

        try
        {
          HashSet<int> reqDatOld = _requestedData;
          _requestedData = new HashSet<int>(requestedValues.RequestedValues);
          OnDataChecking(requestedValues.RequestedValues);
          _requestedData = reqDatOld;
        }
        catch(System.Collections.Generic.KeyNotFoundException)
        {
          PacketSender.SendPacket(PacketSender.Pack(0, 4, 2));
          throw;
        }
        catch(NotSupportedException)
        {
          PacketSender.SendPacket(PacketSender.Pack(0, 4, 3));
          throw;
        }
        catch
        {
          PacketSender.SendPacket(PacketSender.Pack(0, 4, 0xFE));
          throw;
        }


        // TODO: Check for correct data group
        if (_requestedData == null)
          _requestedData = new HashSet<int>(requestedValues.RequestedValues);
        else
          _requestedData.AddRange(requestedValues.RequestedValues);

	if ((ClientPriority  != ClientPriority.Master) ||
	     ((requestedValues == null) || (requestedValues.RequestedDataGroup != 0) ||
             ((requestedValues.RequestedValues != null) && (requestedValues.RequestedValues.Length != 0)))) 
		//ToDo: Improofe Coding-Style. Maybe another location of this statement?
          PacketSender.SendPacket(PacketSender.Pack(0, 4, 0));
      }

      OnDataRequested(_requestedData);
    }

    /// <summary>
    ///   Establish a connection to the TCP server.
    /// </summary>
    /// <param name="hostName">The name or IP address of the host.</param>
    /// <param name="port">The port on the server to connect to (Default: 1435).</param>
    /// <exception cref="ArgumentException">
    ///   This exception is thrown when the host address could
    ///   not be resolved.
    /// </exception>
    public void Connect(string hostName, int port)
    {
      var hostAddresses = Dns.GetHostAddresses(hostName);
      IPAddress myAddress;

      /* The TCP server supports only IPv4, so we have to filter out v6. */
      try
      {
        myAddress = hostAddresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
      }
      catch (Exception ex)
      {
        throw new ArgumentException("Host name could not be resolved.", hostName, ex);
      }

      Connect(new IPEndPoint(myAddress, port));
    }

    /// <summary>
    ///   Establish a connection to the TCP server.
    /// </summary>
    /// <param name="endPoint">Specifies an IP end point to which the interface tries to connect.</param>
    /// <exception cref="ZusiTcpException">
    ///   This exception is thrown when the connection could not be
    ///   established.
    /// </exception>
    protected void Connect(IPEndPoint endPoint)
    {
      TcpClient clientConnection = new TcpClient(AddressFamily.InterNetwork);

      ValidateConnectionState();
      ConnectionState = ConnectionState.Connecting;

      try
      {
        clientConnection.Connect(endPoint);
      }
      catch (SocketException ex)
      {
        throw new ZusiTcpException("Could not establish socket connection to TCP server. " +
                                   "Is the server running and enabled?",
                                   ex);
      }

      InitializeClient(new BinaryIoTcpClient(clientConnection, true));
    }

    protected void SendByteCommand(byte[] array, int id)
    {
      if ((ConnectionState != ConnectionState.Connected) || (!_requestedData.Contains(id)))
      {
        return;
      }
      var ida = new List<byte>(BitConverter.GetBytes(id));
      ida.RemoveAt(3);
      ida.Reverse();
      PacketSender.SendLargePacket(ida.ToArray(), array);
      }

    protected override void ReceiveLoop()
    {
      // TODO: Find out whether there can be something meaningful here and use it.

      ClientConnection.ReadByte();
      // The reader waits until a byte has been received without timeout.
      throw new NotSupportedException("A slave client sent data unexpectedly.");
    }
  }
}
