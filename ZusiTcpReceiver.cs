#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  public abstract class ZusiTcpReceiver : Base_Connection
  {
    private readonly TCPCommands _commands;
    private readonly SwitchableReadOnlyList<int> _requestedData = new SwitchableReadOnlyList<int>();

    protected ZusiTcpReceiver(string clientId,
                              ClientPriority priority,
                              SynchronizationContext hostContext,
                              TCPCommands commands)
      : base(clientId, priority, hostContext)
    {
      if (commands == null)
      {
        throw new ArgumentNullException("commands");
      }
      _commands = commands;
    }

    protected ZusiTcpReceiver(string clientId, ClientPriority priority, TCPCommands commands) : base(clientId, priority)
    {
      if (commands == null)
      {
        throw new ArgumentNullException("commands");
      }
      _commands = commands;
    }

    /// <summary>
    ///   Represents a list of all measurements which will be requested from Zusi on connecting. Add your required measurements
    ///   here before connecting to the server. List is read-only while connected.
    ///   <seealso cref="IDs" />
    /// </summary>
    public virtual ICollection<int> RequestedData
    {
      get { return _requestedData; }
    }

    public override ConnectionState ConnectionState
    {
      get { return base.ConnectionState; }
      protected set
      {
        _requestedData.IsReadOnly = (value == ConnectionState.Connected);
        base.ConnectionState = value;
      }
    }

    /// <summary>
    ///   Represents all measurements available in Zusi as a key-value list. Can be used to convert plain text names of
    ///   measurements to their internal ID.
    /// </summary>
    /// <example>
    ///   <code>
    ///  ZusiTcpConn myConn = [...]
    /// 
    ///  int SpeedID = myConn.IDs["Geschwindigkeit"]
    ///  /* SpeedID now contains the value 01. */
    ///  </code>
    /// </example>
    public ZusiData<string, int> IDs
    {
      get { return _commands.IDByName; }
    }

    /// <summary>
    ///   Represents all measurements available in Zusi as a key-value list. Can be used to convert measurement IDs to their
    ///   plain text name.
    /// </summary>
    /// <example>
    ///   <code>
    ///  ZusiTcpConn myConn = [...]
    /// 
    ///  string SpeedName = myConn.ReverseIDs[1] /* ID 01 == current speed */
    ///  /* SpeedName now contains the value "Geschwindigkeit". */
    ///  </code>
    /// </example>
    public ZusiData<int, string> ReverseIDs
    {
      get { return _commands.NameByID; }
    }

    /// <summary>
    ///   Returns the plain text name of the measurement specified by its ID.
    /// </summary>
    /// <param name="id">Internal ID of the measurement.</param>
    /// <returns>Name of the measurement.</returns>
    public string this[int id]
    {
      get { return ReverseIDs[id]; }
    }

    /// <summary>
    ///   Returns the ID of the measurement specified in plain text.
    /// </summary>
    /// <param name="name">Name of the measurement.</param>
    /// <returns>Internal ID of the measurement.</returns>
    public int this[string name]
    {
      get { return IDs[name]; }
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
      if (RequestedData.Count == 0)
      {
        throw (new ZusiTcpException("No Data marked to be requested. Request Data first."));
      }

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

    /// <summary>
    ///   Request the measurement passed as plain text in the parameter "name" from the server. Shorthand for
    ///   <c>TCP.RequestedData.Add(TCP.IDs[name]);</c>. Must be called before connecting.
    /// </summary>
    /// <param name="name">The name of the measurement.</param>
    /// <exception cref="ZusiTcpException">Thrown, when communciation is not disconnected.</exception>
    public void RequestData(string name)
    {
      if (ConnectionState != ConnectionState.Disconnected)
      {
        throw (new ZusiTcpException("Network state must be \"Disconnect\". Disconnect first!"));
      }
      RequestedData.Add(IDs[name]);
    }

    /// <summary>
    ///   Request the measurement passed as ID in the parameter "id" from the server. Shorthand for
    ///   <c>TCP.RequestedData.Add(id);</c>. Must be called before connecting.
    /// </summary>
    /// <param name="id">The ID of the measurement.</param>
    /// <exception cref="ZusiTcpException">Thrown, when communciation is not disconnected.</exception>
    public void RequestData(int id)
    {
      if (ConnectionState != ConnectionState.Disconnected)
      {
        throw (new ZusiTcpException("Network state must be \"Disconnect\". Disconnect first!"));
      }
      RequestedData.Add(id);
    }

    protected override void HandleHandshake()
    {
      SendLargePacket(
        Pack(0, 1, 2, (byte) ClientPriority, Convert.ToByte(StringEncoder.GetByteCount(ClientId))),
        StringEncoder.GetBytes(ClientId));

      ExpectResponse(ResponseType.AckHello, 0);

      RequestDataFromZusi(RequestedData);

      ExpectResponse(ResponseType.AckNeededData, 0);
    }

    protected override void ReceiveLoop()
    {
      DefaultReceiveLoop();
    }

    protected void DefaultReceiveLoop()
    {
      var dataHandlers = new Dictionary<string, MethodInfo>();

      int bytesReadBase = 2;

      try
      {
        while (ConnectionState == ConnectionState.Connected)
        {
          int packetLength = ClientConnection.ReadInt32();

          int curInstr = GetInstruction(ClientConnection.ReadByte(), ClientConnection.ReadByte());

          if (curInstr < 10)
          {
            throw new ZusiTcpException("Unexpected Non-DATA instruction received.");
          }

          int bytesRead = bytesReadBase; //0 oder 2??????

          while (bytesRead < packetLength)
          {
            int curID = ClientConnection.ReadByte() + 256*curInstr;

            bytesRead += 1;

            CommandEntry curCommand = _commands[curID];

            MethodInfo handlerMethod;

            if (!dataHandlers.TryGetValue(curCommand.Type, out handlerMethod))
            {
              handlerMethod = GetType().GetMethod(
                String.Format("HandleDATA_{0}", curCommand.Type),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] {typeof (IBinaryReader), typeof (int)},
                null);

              if (handlerMethod == null)
              {
                throw new ZusiTcpException(
                  String.Format(
                    "Unknown type {0} for DATA ID {1} (\"{2}\") occured.", curCommand.Type, curID, curCommand.Name));
              }

              /* Make sure the handler method returns an int. */
              Debug.Assert(handlerMethod.ReturnType == typeof (int));

              dataHandlers.Add(curCommand.Type, handlerMethod);
            }

            bytesRead += (int) handlerMethod.Invoke(this, new object[] {ClientConnection, curID});
          }
        }
      }
      catch (EndOfStreamException e)
      {
        /* EndOfStream occurs when the NetworkStream reaches its end while the binaryReader tries to read from it.
         * This happens when the socket closes the stream.
         */

        ZusiTcpException newEx = new ZusiTcpException("Connection to the TCP server has been lost.", e);
        HandleException(newEx);
      }
    }
  }
}
