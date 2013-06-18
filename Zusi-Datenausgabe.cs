/*************************************************************************
 * Zusi-Datenausgabe.cs
 * Contains main logic for the TCP interface.
 *
 * (C) 2009-2011 Andreas Karg, <Clonkman@gmx.de>
 *
 * This file is part of Zusi TCP Interface.NET.
 *
 * Zusi TCP Interface.NET is free software: you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * Zusi TCP Interface.NET is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Zusi TCP Interface.NET.
 * If not, see <http://www.gnu.org/licenses/>.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

[assembly: CLSCompliant(true)]

namespace Zusi_Datenausgabe
{
  /// <summary>
  /// Represents the delegate type required for event handling. Used to transfer incoming data sets to the client application.
  /// </summary>
  /// <param name="data">Contains the new dataset.</param>
  /// <param name="sender">Contains the object triggering the event.</param>
  public delegate void ReceiveEvent<T>(object sender, DataReceivedEventArgs<T> data);

  /// <summary>
  /// Represents the delegate type required for error event handling. Used to handle exceptions that occur in the reception thread.
  /// </summary>
  /// <param name="ex">Contains the exception that has occured.</param>
  /// <param name="sender">Contains the object triggering the event.</param>
  public delegate void ErrorEvent(object sender, ZusiTcpException ex);

  /// <summary>
  /// Represents a structure containing the key and value of one dataset received via the TCP interface.
  /// </summary>
  /// <typeparam name="T">Type of this data set. May be <see cref="float"/>, <see cref="string"/> or <see cref="byte"/>[]</typeparam>
  public class DataReceivedEventArgs<T> : EventArgs
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DataReceivedEventArgs{T}"/> structure and fills the Id and Value fields with the values
    /// passed to the id and value parameters respectively.
    /// </summary>
    /// <param name="id">The id number of the measurement.</param>
    /// <param name="value">The value of the measurement.</param>
    public DataReceivedEventArgs(int id, T value)
    {
      Id = id;
      Value = value;
    }

    /// <summary>
    /// Gets the Zusi ID number of this dataset.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the new value of this dataset.
    /// </summary>
    public T Value { get; private set; }
  }

  /// <summary>
  /// Represents the centerpiece of the Zusi TCP interface.
  ///
  /// <para>Usage:
  /// <list type="number">
  /// <item><description>
  /// Implement event handlers.
  /// All data sent by Zusi are converted to their appropriate types.
  /// </description></item>
  ///
  /// <item><description>
  /// Create an instance of <see cref="ZusiTcpClientConnection"/>, choosing a client priority. Recommended value for control desks is "High".
  /// Add your event handlers to the appropriate events.
  /// </description></item>
  /// <item><description>
  /// Add the required measurements using <see cref="RequestedData"/>.
  /// You can use either the correct ID numbers or the measurements' german names as specified in the TCP server's commandset.xml.
  /// </description></item>
  /// <item><description>
  /// <see cref="Connect(string, int)"/> or <seealso cref="Connect(System.Net.IPEndPoint)"/> to the TCP server.</description></item>
  /// <item><description>As soon as data is coming from the server, the respective events are called automatically, passing one new
  /// dataset at a time.</description></item>
  /// </list></para>
  ///
  /// Notice that ZusiTcpClientConnection implements IDisposable, so remember to dispose of it properly when you are finished.
  /// </summary>
  public class ZusiTcpClientConnection : IDisposable
  {
    #region Fields

    private readonly SynchronizationContext _hostContext;

    private readonly ASCIIEncoding _stringEncoder = new ASCIIEncoding();

    private readonly List<int> _requestedData = new List<int>();

    private TcpClient _clientConnection;
    private NetworkStream _clientStream;
    private BinaryReader _clientReader;

    private Thread _streamReaderThread;

    private readonly TCPCommands _commands;
    private readonly DataReceptionHandler _dataReceptionHandler;

    #endregion

    public event EventHandler<DataReceivedEventArgs<float>> FloatReceived
    {
      add { _dataReceptionHandler.FloatReceived += value; }
      remove { _dataReceptionHandler.FloatReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<string>> StringReceived
    {
      add { _dataReceptionHandler.StringReceived += value; }
      remove { _dataReceptionHandler.StringReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<int>> IntReceived
    {
      add { _dataReceptionHandler.IntReceived += value; }
      remove { _dataReceptionHandler.IntReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<bool>> BoolReceived
    {
      add { _dataReceptionHandler.BoolReceived += value; }
      remove { _dataReceptionHandler.BoolReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<DateTime>> DateTimeReceived
    {
      add { _dataReceptionHandler.DateTimeReceived += value; }
      remove { _dataReceptionHandler.DateTimeReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<DoorState>> DoorsReceived
    {
      add { _dataReceptionHandler.DoorsReceived += value; }
      remove { _dataReceptionHandler.DoorsReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<PZBSystem>> PZBReceived
    {
      add { _dataReceptionHandler.PZBReceived += value; }
      remove { _dataReceptionHandler.PZBReceived -= value; }
    }

    public event EventHandler<DataReceivedEventArgs<BrakeConfiguration>> BrakeConfigReceived
    {
      add { _dataReceptionHandler.BrakeConfigReceived += value; }
      remove { _dataReceptionHandler.BrakeConfigReceived -= value; }
    }

    /// <summary>
    /// Event called when an error has occured within the TCP interface.
    /// </summary>
    public event ErrorEvent ErrorReceived;

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpClientConnection"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    public ZusiTcpClientConnection(string clientId, ClientPriority priority, String commandsetPath = "commandset.xml") :
      this(clientId, priority, TCPCommands.LoadFromFile(commandsetPath))
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpClientConnection"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commands">A set of commands.</param>
    public ZusiTcpClientConnection(string clientId, ClientPriority priority, TCPCommands commands)
    {
      if (SynchronizationContext.Current == null)
      {
        throw new ZusiTcpException("Cannot create TCP connection object: SynchronizationContext.Current is null. " +
                                   "This happens when the object is created before the context is initialized in " +
                                   "Application.Run() or equivalent. " +
                                   "Possible solution: Create object later, e.g. when the user clicks the \"Connect\" button.");
      }

      ClientId = clientId;
      ClientPriority = priority;

      _hostContext = SynchronizationContext.Current;

      _dataReceptionHandler = new DataReceptionHandler(_hostContext);

      _commands = commands;
    }

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
    /// ZusiTcpClientConnection myConn = [...]
    ///
    /// int SpeedID = myConn.IDs["Geschwindigkeit"]
    /// /* SpeedID now contains the value 01. */
    /// </code>
    /// </example>
    public IReadOnlyDictionary<string, int> IDs
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
    /// ZusiTcpClientConnection myConn = [...]
    ///
    /// string SpeedName = myConn.ReverseIDs[1] /* ID 01 == current speed */
    /// /* SpeedName now contains the value "Geschwindigkeit". */
    /// </code>
    /// </example>
    public IReadOnlyDictionary<int, string> ReverseIDs
    {
      get
      {
        return _commands.NameByID;
      }
    }

    /// <summary>
    /// Represents the current connection state of the client.
    /// </summary>
    public ConnectionState ConnectionState { get; private set; }

    /// <summary>
    /// Represents the priority of the client. Cannot be changed after object creation.
    /// </summary>
    public ClientPriority ClientPriority { get; private set; }

    /// <summary>
    /// Represents a list of all measurements requested from Zusi. Add your required measurements
    /// here before connecting to the server.
    /// <seealso cref="IDs"/>
    /// </summary>
    public List<int> RequestedData
    {
      get
      {
        return ConnectionState == ConnectionState.Disconnected ? _requestedData : null;
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

    private void SendPacket(params byte[] message)
    {
      SendToServer(BitConverter.GetBytes(message.Length));
      SendToServer(message);
    }

    private void SendPacket(params byte[][] message)
    {
      int iTempLength = message.Sum(item => item.Length);

      SendToServer(BitConverter.GetBytes(iTempLength));

      foreach (var item in message)
      {
        SendToServer(item);
      }
    }

    private static byte[] Pack(params byte[] message)
    {
      return message;
    }

    private static int GetInstruction(int byteA, int byteB)
    {
      return byteA * 256 + byteB;
    }

    /// <summary>
    /// Establish a connection to the TCP server.
    /// </summary>
    /// <param name="hostName">The name or IP address of the host.</param>
    /// <param name="port">The port on the server to connect to (Default: 1435).</param>
    /// <exception cref="ArgumentException">This exception is thrown when the host address could
    /// not be resolved.</exception>
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
    /// Establish a connection to the TCP server.
    /// </summary>
    /// <param name="endPoint">Specifies an IP end point to which the interface tries to connect.</param>
    /// <exception cref="ZusiTcpException">This exception is thrown when the connection could not be
    /// established.</exception>
    public void Connect(IPEndPoint endPoint)
    {
      try
      {
        if (ConnectionState == ConnectionState.Error)
        {
          throw (new ZusiTcpException("Network state is \"Error\". Disconnect first!"));
        }

        if (_clientConnection == null)
        {
          _clientConnection = new TcpClient(AddressFamily.InterNetwork);
        }

        try
        {
          _clientConnection.Connect(endPoint);
        }
        catch (SocketException ex)
        {
          throw new ZusiTcpException("Could not establish socket connection to TCP server. " +
                                     "Is the server running and enabled?", ex);
        }

        Debug.Assert(_clientConnection.Connected);

        _clientStream = _clientConnection.GetStream();
        _clientReader = new BinaryReader(_clientStream);
        _dataReceptionHandler.ClientReader = _clientReader;

        _streamReaderThread = new Thread(ReceiveLoop) {Name = "ZusiData Receiver"};
        _streamReaderThread.IsBackground = true;

        SendPacket(
          Pack(0, 1, 2, (byte) ClientPriority, Convert.ToByte(_stringEncoder.GetByteCount(ClientId))),
          _stringEncoder.GetBytes(ClientId));

        ExpectAckHello();

        var aGetData = from iData in RequestedData group iData by (iData/256);

        var reqDataBuffer = new List<byte[]>();

        foreach (var aDataGroup in aGetData)
        {
          reqDataBuffer.Clear();
          reqDataBuffer.Add(Pack(0, 3));

          byte[] tempDataGroup = BitConverter.GetBytes(Convert.ToInt16(aDataGroup.Key));
          reqDataBuffer.Add(Pack(tempDataGroup[1], tempDataGroup[0]));

          reqDataBuffer.AddRange(aDataGroup.Select(iID => Pack(Convert.ToByte(iID%256))));

          SendPacket(reqDataBuffer.ToArray());

          ExpectAckNeededData(aDataGroup.Key);
        }

        SendPacket(0, 3, 0, 0);

        ExpectAckNeededData(0);

        ConnectionState = ConnectionState.Connected;

        _streamReaderThread.Start();
      }

      catch
      {
        if (_streamReaderThread != null)
        {
          _streamReaderThread.Abort();
          _streamReaderThread = null;
        }
        ConnectionState = ConnectionState.Error;

        throw;
      }
    }

    private void ExpectAckNeededData(int dataGroup)
    {
      var response = ReceiveResponse(ResponseType.AckNeededData);

      switch (response)
      {
        case 0:
          /* Response is an ACK. */
          return;
        case 1:
          throw new ZusiTcpException("Unknown instruction set: " + dataGroup);
        case 2:
          throw new ZusiTcpException("Client not connected");
        default:
          throw new ZusiTcpException("NEEDED_DATA not acknowledged.");
      }
    }

    private void ExpectAckHello()
    {
      var response = ReceiveResponse(ResponseType.AckHello);

      switch (response)
      {
        case 0:
          /* Response is an ACK. */
          return;
        case 1:
          throw new ZusiTcpException("Too many connections.");
        case 2:
          throw new ZusiTcpException("Zusi is already connected. No more connections allowed.");
        default:
          throw new ZusiTcpException("HELLO not acknowledged.");
      }
    }

    /// <summary>
    /// Disconnect from the TCP server.
    /// </summary>
    public void Disconnnect()
    {
      if((_streamReaderThread != null)&&(_streamReaderThread != Thread.CurrentThread))
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

    private int ReceiveResponse(ResponseType expectedInstruction)
    {
      int packetLength = _clientReader.ReadInt32();
      if (packetLength != 3)
      {
        throw new ZusiTcpException("Invalid packet length: " + packetLength);
      }

      int readInstr = GetInstruction(_clientReader.ReadByte(), _clientReader.ReadByte());
      if (readInstr != (int) expectedInstruction)
      {
        throw new ZusiTcpException("Invalid command from server: " + readInstr);
      }

      int response = _clientReader.ReadByte();
      return response;
    }

    private void ReceiveLoop()
    {
      try
      {
        ReceiveLoopCore();
      }
      catch (Exception e)
      {
        HandleReceiveLoopException(e);
      }
    }

    private void ReceiveLoopCore()
    {
      while (ConnectionState == ConnectionState.Connected)
      {
        ReceivePacket();
      }
    }

    private void HandleReceiveLoopException(Exception e)
    {
      Disconnnect();
      ConnectionState = ConnectionState.Error;

      if (e is ZusiTcpException)
      {
        _hostContext.Post(ErrorMarshal, e as ZusiTcpException);
      }
      else if (e is EndOfStreamException)
      {
        /* EndOfStream occurs when the NetworkStream reaches its end while the binaryReader tries to read from it.
         * This happens when the socket closes the stream.
         */
        var newEx = new ZusiTcpException("Connection to the TCP server has been lost.", e);
        _hostContext.Post(ErrorMarshal, newEx);
      }
      else if (e is ThreadAbortException)
      {
        /* The thread has been killed. Nothing to do. */
      }
      else
      {
        var newEx =
          new ZusiTcpException(
            "An unhandled exception has occured in the TCP receiving loop. This is very probably " +
            "a bug in the Zusi TCP interface for .NET. Please report this error to the author(s) " +
            "of this application and/or the author(s) of the Zusi TCP interface for .NET.", e);

        _hostContext.Post(ErrorMarshal, newEx);
      }
    }

    private void ReceivePacket()
    {
      int packetLength = _clientReader.ReadInt32();

      int curInstr = GetInstruction(_clientReader.ReadByte(), _clientReader.ReadByte());

      if (curInstr < 10)
      {
        throw new ZusiTcpException("Unexpected Non-DATA instruction received.");
      }

      // The first 2 bytes have been the instruction.
      int bytesRead = 2;

      while (bytesRead < packetLength)
      {
        bytesRead += ReceiveDataSegment(curInstr);
      }

      Debug.Assert(bytesRead == packetLength);
    }

    private int ReceiveDataSegment(int curInstr)
    {
      int curID = _clientReader.ReadByte() + 256*curInstr;

      // One byte read for curID
      int bytesRead = 1;

      CommandEntry curCommand = _commands[curID];

      bytesRead += _dataReceptionHandler.HandleData(curCommand, curID);
      return bytesRead;
    }

    private MethodInfo GetHandlerMethod(CommandEntry curCommand, int curID)
    {
      return _dataReceptionHandler.GetHandlerMethod(curCommand, curID);
    }

    /// <summary>
    /// Request the measurement passed as plain text in the parameter "name" from the server. Shorthand for
    /// <c>TCP.RequestedData.Add(TCP.IDs[name]);</c>.
    /// </summary>
    /// <param name="name">The name of the measurement.</param>
    public void RequestData(string name)
    {
      _requestedData.Add(IDs[name]);
    }

    /// <summary>
    /// Request the measurement passed as ID in the parameter "id" from the server. Shorthand for
    /// <c>TCP.RequestedData.Add(id);</c>.
    /// </summary>
    /// <param name="id">The ID of the measurement.</param>
    public void RequestData(int id)
    {
      _requestedData.Add(id);
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

    private enum ResponseType
    {
      None = 0,

      AckHello = 2,

      AckNeededData = 4
    }

    #endregion
  }

  /// <summary>
  /// Represents the state of a TCP connection.
  /// </summary>
  public enum ConnectionState
  {
    /// <summary>
    /// There is no connection to a server.
    /// </summary>
    Disconnected = 0,

    /// <summary>
    /// A connection to a server has been established.
    /// </summary>
    Connected,

    /// <summary>
    /// An error has occured. Try disconnecting and then connecting again to solve the problem.
    /// </summary>
    Error,
  }

  /// <summary>
  /// Represents the priority of the client in the Zusi TCP interface. Determines measurement update freqency.
  /// </summary>
  public enum ClientPriority
  {
    /// <summary>
    /// Undefined priority.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Reserved for Zusi.
    /// </summary>
    Master = 01,

    /// <summary>
    /// High priority for control desks and display applications.
    /// </summary>
    High = 02,

    /// <summary>
    /// Medium priority.
    /// </summary>
    Medium = 03,

    /// <summary>
    /// Low priority.
    /// </summary>
    Low = 04,

    /// <summary>
    /// Maximum priority possible.
    /// </summary>
    RealTime = 05
  }
}