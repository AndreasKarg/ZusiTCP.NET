#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  /// <summary>
  ///   Represents a baseclass for all TCPconnections based on the Zusi-protocol.
  /// </summary>
  public abstract class Base_Connection : IDisposable
  {
    #region Fields

    protected readonly SynchronizationContext HostContext;

    protected readonly Encoding StringEncoder = Encoding.Default;

    protected IBinaryIO ClientConnection;

    private ConnectionState _connectionState = ConnectionState.Disconnected;
    private Thread _streamReaderThread;

    #endregion

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    protected Base_Connection(string clientId, ClientPriority priority, SynchronizationContext hostContext)
    {
      ClientId = clientId;
      ClientPriority = priority;

      HostContext = hostContext;
    }

    /// <summary>
    ///   Initializes a new <see cref="ZusiTcpConn" /> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    protected Base_Connection(string clientId, ClientPriority priority)
      : this(clientId, priority, SynchronizationContext.Current)
    {
      if (SynchronizationContext.Current == null)
      {
        throw new ObjectUnsynchronisableException();
      }
    }

    /// <summary>
    ///   Represents the name of the client.
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    ///   Represents the current connection state of the client.
    /// </summary>
    public virtual ConnectionState ConnectionState
    {
      get { return _connectionState; }
      protected set
      {
        if (_connectionState == value) return; //Connection State did not change.
        _connectionState = value;
        if (HostContext != null)
        {
          HostContext.Post(ConnectMarshal, new EventArgs());
        }
        else
        {
          ConnectMarshal(new EventArgs());
        }
      }
    }

    /// <summary>
    ///   Represents the priority of the client. Cannot be changed after object creation.
    /// </summary>
    public ClientPriority ClientPriority { get; private set; }

    #region IDisposable Members

    /// <summary>
    ///   Dispose of the TCP connection.
    /// </summary>
    public void Dispose()
    {
      Disconnect();
    }

    #endregion

    /// <summary>
    ///   Event called when an error has occured within the TCP interface.
    /// </summary>
    public event ErrorEvent ErrorReceived;

    /// <summary>
    ///   Event called the ConnectionState changed.
    /// </summary>
    public event EventHandler ConnectionState_Changed;

    private void ErrorMarshal(object o)
    {
      if (ErrorReceived == null)
      {
        return;
      }

      ErrorReceived.Invoke(this, (ZusiTcpException) o);
    }

    private void ConnectMarshal(object o)
    {
      if (ConnectionState_Changed == null)
      {
        return;
      }

      ConnectionState_Changed.Invoke(this, (EventArgs) o);
    }

    protected void SendPacket(params byte[] message)
    {
      ClientConnection.SendToPeer(PackArrays(new byte[][] {BitConverter.GetBytes(message.Length), message}));
    }

    protected void SendLargePacket(params byte[][] message)
    {
      int iTempLength = message.Sum(item => item.Length);

      ClientConnection.SendToPeer(PackArrays(BitConverter.GetBytes(iTempLength), message));
    }

    protected static byte[] Pack(params byte[] message)
    {
      return message;
    }
    protected static byte[] PackArrays(byte[] firstPacket, byte[][] otherPackets)
    {
      byte[][] partialPackets = new byte[otherPackets.Length + 1][];
      partialPackets[0] = firstPacket;
      Array.Copy(otherPackets, 0, partialPackets, 1, otherPackets.Length);
      return PackArrays(partialPackets);
    }
    protected static byte[] PackArrays(byte[][] partialPackets)
    {
      byte[] value = new byte[partialPackets.Sum(item => item.Length)];
      int curLoc = 0;
      foreach(byte[] arr in partialPackets)
      {
        Array.Copy(arr, 0, value, curLoc, arr.Length);
        curLoc += arr.Length;
      }
      return value;
    }

    protected static int GetInstruction(int byteA, int byteB)
    {
      return byteA*256 + byteB;
    }

    public void InitializeClient(IBinaryIO clientConnection)
    {
      Debug.Assert(clientConnection.Connected);
      if (ClientConnection != null) throw new System.InvalidOperationException(); //Already initialized.
      ClientConnection = clientConnection;

      try
      {
        _streamReaderThread = new Thread(ReceiveLoopWrapper) {Name = "ZusiData Receiver", IsBackground = true};

        HandleHandshake();

        ConnectionState = ConnectionState.Connected;

        _streamReaderThread.Start();
      }
      catch (Exception e)
      {
        HandleException(new ZusiTcpException("The connection can't be established.", e));
      }
    }

    protected void HandleException(ZusiTcpException e)
    {
      PostExToHost(e);

      Disconnect(ConnectionState.Error);
    }

    protected abstract void HandleHandshake();

    protected void RequestDataFromZusi(IEnumerable<int> requestedData)
    {
      var aGetData = from iData in requestedData
                     group iData by (iData/256);

      var reqDataBuffer = new List<byte[]>();

      foreach (var aDataGroup in aGetData)
      {
        reqDataBuffer.Clear();
        reqDataBuffer.Add(Pack(0, 3));

        var tempDataGroup = BitConverter.GetBytes(Convert.ToInt16(aDataGroup.Key));
        reqDataBuffer.Add(Pack(tempDataGroup[1], tempDataGroup[0]));

        reqDataBuffer.AddRange(aDataGroup.Select(iID => Pack(Convert.ToByte(iID%256))));

        SendLargePacket(reqDataBuffer.ToArray());

        ExpectResponse(ResponseType.AckNeededData, aDataGroup.Key);
      }

      SendPacket(0, 3, 0, 0);
    }

    /// <summary>
    ///   Disconnect from the TCP server.
    /// </summary>
    public void Disconnect()
    {
      Disconnect(ConnectionState.Disconnected);
    }
    private void Disconnect(ConnectionState reason)
    {
      if ((_streamReaderThread != null) && (_streamReaderThread != Thread.CurrentThread))
      {
        _streamReaderThread.Abort();
      }
      ConnectionState = reason;
      if (ClientConnection != null)
      {
        ClientConnection.Close();
      }
      ClientConnection = null;
    }

    /// <summary>
    ///   Blocks until the partner responses.
    /// </summary>
    /// <param name="expResponse">The type of Command the method waits for.</param>
    /// <param name="dataGroup">Data group to specify Error messages when expResponse is ResponseType.AckNeededData.</param>
    /// <exception cref="ArgumentOutOfRangeException">expResponse-Type not supported</exception>
    /// <exception cref="ZusiTcpException">Can't continue connection, reasond: see message.</exception>
    /// <returns>An array with the requested Types for ResponseType.NeededData, null for other values.</returns>
    protected ExpectResponseAnswer ExpectResponse(ResponseType expResponse, int dataGroup)
    {
      int iPacketLength = ClientConnection.ReadInt32();
      if (
        !((iPacketLength == 3) || ((expResponse == ResponseType.Hello) && (iPacketLength >= 5)) ||
          ((expResponse == ResponseType.NeededData) && (iPacketLength >= 4))))
      {
        throw new ZusiTcpException("Invalid packet length: " + iPacketLength);
      }

      int iReadInstr = GetInstruction(ClientConnection.ReadByte(), ClientConnection.ReadByte());
      if (iReadInstr != (int) expResponse)
      {
        throw new ZusiTcpException("Invalid command from server: " + iReadInstr);
      }

      switch (expResponse)
      {
        case ResponseType.None:
          break;
        case ResponseType.AckHello:
        case ResponseType.AckNeededData:
          int iResponse = ClientConnection.ReadByte();
          if (iResponse == 0)
          {
            /* Response is an ACK */
            return null;
          }

          switch (expResponse)
          {
            case ResponseType.AckHello:
              switch (iResponse)
              {
                case 1:
                  throw new ZusiTcpException("Too many connections.");
                case 2:
                  throw new ZusiTcpException("Zusi is already connected. No more connections allowed.");
                default:
                  throw new ZusiTcpException("HELLO not acknowledged.");
              }

            case ResponseType.AckNeededData:
              switch (iResponse)
              {
                case 1:
                  throw new ZusiTcpException("Unknown instruction set: " + dataGroup);
                case 2:
                  throw new ZusiTcpException("Client not connected");
                case 3:
                  throw new ZusiTcpException("Client requested dataset " + dataGroup +
                                             " which was not requested when Zusi was connected.");
                  //Betrifft nur für die neue Server-Implentierung!
                default:
                  throw new ZusiTcpException("NEEDED_DATA not acknowledged.");
              }
          }
          break;
        case ResponseType.Hello:
          int iVersion = ClientConnection.ReadByte();
          if (iVersion > 2)
          {
            throw new ZusiTcpException("Version not Supported.");
          }
          ClientPriority = (ClientPriority) ClientConnection.ReadByte();
          ClientId = ClientConnection.ReadString();
          if (iPacketLength != (5 + ClientId.Length))
          {
            throw new ZusiTcpException("Invalid packet length: " + iPacketLength +
                                       " (Details: Client name didn't keep to length.)");
          }
          break;
        case ResponseType.NeededData:
          int instructionGroup = GetInstruction(ClientConnection.ReadByte(), ClientConnection.ReadByte());
          var requestedTypes = new List<int>();
          for (int i = 4; i < iPacketLength; i++)
          {
            requestedTypes.Add(GetInstruction(instructionGroup, ClientConnection.ReadByte()));
          }
          return new ExpectResponseAnswer(requestedTypes.ToArray(), instructionGroup);
        default:
          throw new ArgumentOutOfRangeException("expResponse");
      }
      return null;
    }

    private void ReceiveLoopWrapper()
    {
      try
      {
        ReceiveLoop();
      }
      catch (ZusiTcpException e)
      {
        HandleException(e);
      }
      catch (Exception e)
      {
        ZusiTcpException newEx =
          new ZusiTcpException(
            "An unhandled exception has occured in the TCP receiving loop. This is very probably " +
            "a bug in the Zusi TCP interface for .NET. Please report this error to the author(s) " +
            "of this application and/or the author(s) of the Zusi TCP interface for .NET.",
            e);

        HandleException(newEx); //ToDo: Why was this only "PostEx"? This had caused Error While ConnectionState Connected!
      }
    }

    protected abstract void ReceiveLoop();

    private void EventMarshal<T>(object o)
    {
      var margs = (MarshalArgs<T>) o;
      margs.Event.Invoke(this, margs.Data);
    }

    /// <summary>
    ///   When you have received a data packet from the server and are done
    ///   processing it in a HandleDATA-routine, call PostToHost() to trigger
    ///   an event for this type.
    /// </summary>
    /// <typeparam name="T">
    ///   Contains the data type for which the event is thrown.
    ///   This can be safely ommitted.
    /// </typeparam>
    /// <param name="Event">Contains the event that is to be thrown.</param>
    /// <param name="id">Contains the Zusi command ID.</param>
    /// <param name="value">Contains the new value of the measure.</param>
    protected void PostToHost<T>(ReceiveEvent<T> Event, int id, T value)
    {
      if (Event == null)
      {
        return;
      }

      if (HostContext != null)
      {
        HostContext.Post(EventMarshal<T>, new MarshalArgs<T>(Event, id, value));
      }
      else
      {
        EventMarshal<T>(new MarshalArgs<T>(Event, id, value));
      }
    }

    /// <summary>
    ///   Raises the ErrorReceived-event.
    /// </summary>
    /// <param name="ex">The exception.</param>
    private void PostExToHost(ZusiTcpException ex)
    {
      if (HostContext != null)
      {
        HostContext.Post(ErrorMarshal, ex);
      }
      else
      {
        ErrorMarshal(ex);
      }
    }

    protected void ValidateConnectionState()
    {
      if (ConnectionState == ConnectionState.Error)
      {
        throw (new ZusiTcpException("Network state is \"Error\". Disconnect first!"));
      }
    }

    #region Nested type: ResponseType

    protected enum ResponseType
    {
      None = 0,
      Hello = 1,
      AckHello = 2,
      NeededData = 3,
      AckNeededData = 4
    }

    #endregion

    #region Nested type: ExpectResponseAnswer

    protected class ExpectResponseAnswer
    {
      public ExpectResponseAnswer(int[] requestedValues, int requestedDataGroup)
      {
        RequestedValues = requestedValues;
        RequestedDataGroup = requestedDataGroup;
      }

      public int[] RequestedValues { get; private set; }
      public int RequestedDataGroup { get; private set; }
    }

    #endregion

    private struct MarshalArgs<T>
    {
      public MarshalArgs(ReceiveEvent<T> recveiveEvent, int id, T data)
        : this()
      {
        Event = recveiveEvent;
        Data = new DataSet<T>(id, data);
      }

      public ReceiveEvent<T> Event { get; private set; }

      public DataSet<T> Data { get; private set; }
    }
  }
}
