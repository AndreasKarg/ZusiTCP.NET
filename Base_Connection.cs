using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Zusi_Datenausgabe
{
  /// <summary>
  /// Represents a baseclass for all TCPconnections based on the Zusi-protocol.
  /// </summary>
  public abstract class Base_Connection : IDisposable
  {
    #region Fields

    private readonly SynchronizationContext _hostContext;

    protected readonly ASCIIEncoding StringEncoder = new ASCIIEncoding();

    private readonly SwitchableReadOnlyList<int> _requestedData = new SwitchableReadOnlyList<int>();

    private TcpClient _clientConnection;
    private NetworkStream _clientStream;
    private BinaryReader _clientReader;

    private Thread _streamReaderThread;

    private readonly TCPCommands _commands;
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    #endregion

    /// <summary>
    /// Event called when an error has occured within the TCP interface.
    /// </summary>
    public event ErrorEvent ErrorReceived;

    /// <summary>
    /// Event called the ConnectionState changed.
    /// </summary>
    public event EventHandler ConnectionState_Changed;



    /// <summary>
    /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    protected Base_Connection(string clientId, ClientPriority priority, TCPCommands commandsetDocument, SynchronizationContext hostContext)
    {
      if (commandsetDocument == null)
        throw new ZusiTcpException("Cannot create TCP connection object: commandsetDocument is null. ");
      ClientId = clientId;
      ClientPriority = priority;

      _hostContext = hostContext;

      _commands = commandsetDocument;
    }

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetDocument">The XML file containig the command set.</param>
    protected Base_Connection(string clientId, ClientPriority priority, TCPCommands commandsetDocument)
      : this(clientId, priority, commandsetDocument, SynchronizationContext.Current)
    {
      if (SynchronizationContext.Current == null)
      {
        throw new ZusiTcpException("Cannot create TCP connection object: SynchronizationContext.Current is null. " +
                                   "This happens when the object is created before the context is initialized in " +
                                   "Application.Run() or equivalent. " +
                                   "Possible solution: Create object later, e.g. when the user clicks the \"Connect\" button.");
      }
    }

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    protected Base_Connection(string clientId, ClientPriority priority, string commandsetPath, SynchronizationContext hostContext)
      : this(clientId, priority, TCPCommands.LoadFromFile(commandsetPath), hostContext)
    { }

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    protected Base_Connection(string clientId, ClientPriority priority, string commandsetPath)
      : this(clientId, priority, TCPCommands.LoadFromFile(commandsetPath))
    { }

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    protected Base_Connection(string clientId, ClientPriority priority, SynchronizationContext hostContext)
      : this(clientId, priority, "commandset.xml", hostContext)
    { }

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpConn"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    protected Base_Connection(string clientId, ClientPriority priority)
      : this(clientId, priority, "commandset.xml")
    { }



    /// <summary>
    /// Represents the name of the client.
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    /// Represents all measurements available in Zusi as a key-value list. Can be used to convert plain text names of
    /// measurements to their internal ID.
    /// </summary>
    /// <example>
    /// <code>
    /// ZusiTcpConn myConn = [...]
    ///
    /// int SpeedID = myConn.IDs["Geschwindigkeit"]
    /// /* SpeedID now contains the value 01. */
    /// </code>
    /// </example>
    public ZusiData<string, int> IDs
    {
      get
      {
        return _commands.IDByName;
      }
    }

    /// <summary>
    /// Represents all measurements available in Zusi as a key-value list. Can be used to convert measurement IDs to their
    /// plain text name.
    /// </summary>
    /// <example>
    /// <code>
    /// ZusiTcpConn myConn = [...]
    ///
    /// string SpeedName = myConn.ReverseIDs[1] /* ID 01 == current speed */
    /// /* SpeedName now contains the value "Geschwindigkeit". */
    /// </code>
    /// </example>
    public ZusiData<int, string> ReverseIDs
    {
      get
      {
        return _commands.NameByID;
      }
    }

    /// <summary>
    /// Represents the current connection state of the client.
    /// </summary>
    public ConnectionState ConnectionState
    {
      get
      {
        return _connectionState;
      }
      protected set
      {
        _connectionState = value;
        _requestedData.IsReadOnly = (value == ConnectionState.Connected);
        if (_hostContext != null)
          _hostContext.Post(ConnectMarshal, new EventArgs());
        else
          ConnectMarshal(new EventArgs());
      }
    }

    /// <summary>
    /// Represents the priority of the client. Cannot be changed after object creation.
    /// </summary>
    public ClientPriority ClientPriority { get; private set; }

    /// <summary>
    /// Represents a list of all measurements which will be requested from Zusi on connecting. Add your required measurements
    /// here before connecting to the server. List is read-only while connected.
    /// <seealso cref="IDs"/>
    /// </summary>
    public ICollection<int> RequestedData
    {
      get
      {
        return _requestedData;
      }
    }

    /// <summary>
    /// Returns the ID of the measurement specified in plain text.
    /// </summary>
    /// <param name="name">Name of the measurement.</param>
    /// <returns>Internal ID of the measurement.</returns>
    public int this[string name]
    {
      get
      {
        return IDs[name];
      }
    }

    /// <summary>
    /// Returns the plain text name of the measurement specified by its ID.
    /// </summary>
    /// <param name="id">Internal ID of the measurement.</param>
    /// <returns>Name of the measurement.</returns>
    public string this[int id]
    {
      get
      {
        return ReverseIDs[id];
      }
    }

    #region IDisposable Members

    /// <summary>
    /// Dispose of the TCP connection.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    private void ErrorMarshal(object o)
    {
      if (ErrorReceived == null)
      {
        return;
      }

      ErrorReceived.Invoke(this, (ZusiTcpException)o);
    }

    private void ConnectMarshal(object o)
    {
      if (ConnectionState_Changed == null)
      {
        return;
      }

      ConnectionState_Changed.Invoke(this, (EventArgs)o);
    }

    private void SendToServer(byte[] message)
    {
      try
      {
        _clientStream.Write(message, 0, message.Length);
      }
      catch (IOException ex)
      {
        throw new ZusiTcpException("An error occured when trying to send data to the server.", ex);
      }
    }

    protected void SendPacket(params byte[] message)
    {
      SendToServer(BitConverter.GetBytes(message.Length));
      SendToServer(message);
    }

    protected void SendLargePacket(params byte[][] message)
    {
      int iTempLength = message.Sum(item => item.Length);

      SendToServer(BitConverter.GetBytes(iTempLength));

      foreach (var item in message)
      {
        SendToServer(item);
      }
    }

    protected static byte[] Pack(params byte[] message)
    {
      return message;
    }

    private static int GetInstruction(int byteA, int byteB)
    {
      return byteA * 256 + byteB;
    }

    protected void InitializeClient(TcpClient clientConnection)
    {
      Debug.Assert(clientConnection.Connected);

      _clientStream = clientConnection.GetStream();
      _clientReader = new BinaryReader(_clientStream, StringEncoder);

      try
      {
        _streamReaderThread = new Thread(ReceiveLoop) { Name = "ZusiData Receiver", IsBackground = true };

        HandleHandshake();

        ConnectionState = ConnectionState.Connected;

        _streamReaderThread.Start();
      }
      catch
      {
        HandleError();
        throw;
      }
    }

    private void HandleError()
    {
      if (_streamReaderThread != null)
        _streamReaderThread.Abort();
      _streamReaderThread = null;

      ConnectionState = ConnectionState.Error;
    }

    protected abstract void HandleHandshake();

    protected void RequestDataFromZusi()
    {
      var aGetData = from iData in RequestedData group iData by (iData / 256);

      var reqDataBuffer = new List<byte[]>();

      foreach (var aDataGroup in aGetData)
      {
        reqDataBuffer.Clear();
        reqDataBuffer.Add(Pack(0, 3));

        byte[] tempDataGroup = BitConverter.GetBytes(Convert.ToInt16(aDataGroup.Key));
        reqDataBuffer.Add(Pack(tempDataGroup[1], tempDataGroup[0]));

        reqDataBuffer.AddRange(aDataGroup.Select(iID => Pack(Convert.ToByte(iID % 256))));

        SendLargePacket(reqDataBuffer.ToArray());

        ExpectResponse(ResponseType.AckNeededData, aDataGroup.Key);
      }

      SendPacket(0, 3, 0, 0);
    }

    /// <summary>
    /// Disconnect from the TCP server.
    /// </summary>
    public void Disconnnect()
    {
      if ((_streamReaderThread != null) && (_streamReaderThread != Thread.CurrentThread))
      {
        _streamReaderThread.Abort();
      }
      ConnectionState = ConnectionState.Disconnected;
      if (_clientConnection != null)
      {
        _clientConnection.Close();
      }
      _clientConnection = null;
    }

    /// <summary>
    /// Blocks until the partner responses.
    /// </summary>
    /// <param name="expResponse">The type of Command the method waits for.</param>
    /// <param name="dataGroup">Data group to specify Error messages when expResponse is ResponseType.AckNeededData.</param>
    /// <exception cref="ArgumentOutOfRangeException">expResponse-Type not supported</exception>
    /// <exception cref="ZusiTcpException">Can't continue connection, reasond: see message.</exception>
    /// <returns>An array with the requested Types for ResponseType.NeededData, null for other values.</returns>
    protected ExpectResponseAnswer ExpectResponse(ResponseType expResponse, int dataGroup)
    {
      int iPacketLength = _clientReader.ReadInt32();
      if (!((iPacketLength == 3) || ((expResponse == ResponseType.Hello) && (iPacketLength >= 5)) || ((expResponse == ResponseType.NeededData) && (iPacketLength >= 4))))
      {
        throw new ZusiTcpException("Invalid packet length: " + iPacketLength);
      }

      int iReadInstr = GetInstruction(_clientReader.ReadByte(), _clientReader.ReadByte());
      if (iReadInstr != (int)expResponse)
      {
        throw new ZusiTcpException("Invalid command from server: " + iReadInstr);
      }

      switch (expResponse)
      {
        case ResponseType.None:
          break;
        case ResponseType.AckHello:
        case ResponseType.AckNeededData:
          int iResponse = _clientReader.ReadByte();
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
                  throw new ZusiTcpException("Client requested dataset " + dataGroup + " which was not requested when Zusi was connected."); //Betrifft nur für die neue Server-Implentierung!
                default:
                  throw new ZusiTcpException("NEEDED_DATA not acknowledged.");
              }
          }
          break;
        case ResponseType.Hello:
          int iVersion = _clientReader.ReadByte();
          if (iVersion > 2)
          {
            throw new ZusiTcpException("Version not Supported.");
          }
          this.ClientPriority = (ClientPriority)_clientReader.ReadByte();
          this.ClientId = _clientReader.ReadString();
          if (iPacketLength != (5 + ClientId.Length))
          {
            throw new ZusiTcpException("Invalid packet length: " + iPacketLength + " (Details: Client name didn't keep to length.)");
          }
          break;
        case ResponseType.NeededData:
          int area = GetInstruction(_clientReader.ReadByte(), _clientReader.ReadByte());
          List<int> requestedTypes = new List<int>();
          for (int i = 4; i < iPacketLength; i++)
            requestedTypes.Add(GetInstruction(area, _clientReader.ReadByte()));
          return new ExpectResponseAnswer(requestedTypes.ToArray(), area);
        default:
          throw new ArgumentOutOfRangeException("expResponse");
      }
      return null;
    }

    protected abstract void ReceiveLoop();

    protected void DefaultReceiveLoop()
    {
      var dataHandlers = new Dictionary<string, MethodInfo>();

      int bytesReadBase = 2;

      try
      {
        while (ConnectionState == ConnectionState.Connected)
        {
          int packetLength = _clientReader.ReadInt32();

          int curInstr = GetInstruction(_clientReader.ReadByte(), _clientReader.ReadByte());

          if (curInstr < 10)
          {
            throw new ZusiTcpException("Unexpected Non-DATA instruction received.");
          }

          int bytesRead = bytesReadBase; //0 oder 2??????

          while (bytesRead < packetLength)
          {
            int curID = _clientReader.ReadByte() + 256 * curInstr;

            bytesRead += 1;

            CommandEntry curCommand = _commands[curID];

            MethodInfo handlerMethod;

            if (!dataHandlers.TryGetValue(curCommand.Type, out handlerMethod))
            {
              handlerMethod = GetType().GetMethod(
                  String.Format("HandleDATA_{0}", curCommand.Type),
                  BindingFlags.Instance | BindingFlags.NonPublic,
                  null,
                  new[] { typeof(BinaryReader), typeof(int) },
              null);

              if (handlerMethod == null)
              {
                throw new ZusiTcpException(
                    String.Format(
                    "Unknown type {0} for DATA ID {1} (\"{2}\") occured.", curCommand.Type, curID, curCommand.Name));
              }

              /* Make sure the handler method returns an int. */
              Debug.Assert(handlerMethod.ReturnType == typeof(int));

              dataHandlers.Add(curCommand.Type, handlerMethod);
            }

            bytesRead += (int)handlerMethod.Invoke(this, new object[] { _clientReader, curID });
          }
        }
      }
      catch (Exception e)
      {
        Disconnnect();
        ConnectionState = ConnectionState.Error;

        if (e is ZusiTcpException)
        {
          PostExToHost(e as ZusiTcpException);
        }
        else if (e is EndOfStreamException)
        {
          /* EndOfStream occurs when the NetworkStream reaches its end while the binaryReader tries to read from it.
 * This happens when the socket closes the stream.
 */
          var newEx = new ZusiTcpException("Connection to the TCP server has been lost.", e);
          PostExToHost(newEx);
        }
        else
        {
          var newEx =
              new ZusiTcpException(
                  "An unhandled exception has occured in the TCP receiving loop. This is very probably " +
                  "a bug in the Zusi TCP interface for .NET. Please report this error to the author(s) " +
                  "of this application and/or the author(s) of the Zusi TCP interface for .NET.", e);

          PostExToHost(newEx);
        }
      }
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
      {
        return;
      }

      if (_clientConnection != null)
      {
        _clientConnection.Close();
        _clientConnection = null;
      }

      if (_streamReaderThread == null)
      {
        return;
      }

      _streamReaderThread.Abort();
      _streamReaderThread = null;
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
      public ExpectResponseAnswer(int[] requestArray, int requestArea)
      {
        this.requestArray = requestArray;
        this.requestArea = requestArea;
      }
      public int[] requestArray { get; private set; }
      public int requestArea { get; private set; }
    }

    #endregion


    private struct MarshalArgs<T>
    {
      public ReceiveEvent<T> Event { get; private set; }

      public DataSet<T> Data { get; private set; }

      public MarshalArgs(ReceiveEvent<T> recveiveEvent, int id, T data)
        : this()
      {
        Event = recveiveEvent;
        Data = new DataSet<T>(id, data);
      }
    }

    private void EventMarshal<T>(object o)
    {
      var margs = (MarshalArgs<T>)o;
      margs.Event.Invoke(this, margs.Data);
    }

    /// <summary>
    /// When you have received a data packet from the server and are done
    /// processing it in a HandleDATA-routine, call PostToHost() to trigger
    /// an event for this type.
    /// </summary>
    /// <typeparam name="T">Contains the data type for which the event is thrown.
    /// This can be safely ommitted.</typeparam>
    /// <param name="Event">Contains the event that is to be thrown.</param>
    /// <param name="id">Contains the Zusi command ID.</param>
    /// <param name="value">Contains the new value of the measure.</param>
    protected void PostToHost<T>(ReceiveEvent<T> Event, int id, T value)
    {
      if (Event == null)
      {
        return;
      }

      if (_hostContext != null)
        _hostContext.Post(EventMarshal<T>, new MarshalArgs<T>(Event, id, value));
      else
        EventMarshal<T>(new MarshalArgs<T>(Event, id, value));

    }

    /// <summary>
    /// Raises the ErrorReceived-event.
    /// </summary>
    /// <param name="ex">The exception.</param>
    protected void PostExToHost(ZusiTcpException ex)
    {
      if (_hostContext != null)
        _hostContext.Post(ErrorMarshal, ex as ZusiTcpException);
      else
        ErrorMarshal(ex);
    }

    protected void ValidateConnectionState()
    {
      if (ConnectionState == ConnectionState.Error)
      {
        throw (new ZusiTcpException("Network state is \"Error\". Disconnect first!"));
      }
    }
  }
}

