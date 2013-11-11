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
using System.Text;
using System.Threading;
using Zusi_Datenausgabe.AuxiliaryClasses;
using Zusi_Datenausgabe.DataReader;
using Zusi_Datenausgabe.EventManager;
using Zusi_Datenausgabe.NetworkIO;
using Zusi_Datenausgabe.ReadOnlyDictionary;
using Zusi_Datenausgabe.TcpCommands;

[assembly: CLSCompliant(true)]

namespace Zusi_Datenausgabe
{
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
  public class ZusiTcpClientConnection : IZusiTcpClientConnection, IDisposable
  {
    #region Fields
    // TODO: DIfy this class
    private readonly SynchronizationContext _hostContext;

    private readonly List<int> _requestedData = new List<int>();

    private Thread _streamReaderThread;

    private readonly ITcpCommandDictionary _commands;
    private readonly IDataReceptionHandler _dataReceptionHandler;
    private INetworkIOHandler _networkIOHandler;
    private INetworkIOHandlerFactory _networkHandlerFactory;

    private ITypedAndGenericEventManager<int> _eventManager;

    #endregion

    #region Delegating methods for _eventManager

    public void Subscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _eventManager.Subscribe(handler);
    }

    public void Unsubscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _eventManager.Unsubscribe(handler);
    }

    public void Subscribe<T>(int id, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _eventManager.Subscribe(id, handler);
    }

    public void Unsubscribe<T>(int id, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _eventManager.Unsubscribe(id, handler);
    }

    #endregion

    /// <summary>
    /// Event called when an error has occured within the TCP interface.
    /// </summary>
    public event EventHandler<ErrorEventArgs> ErrorReceived;

    /// <summary>
    /// Initializes a new <see cref="ZusiTcpClientConnection"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="dictionaryFactory">A factory method that takes a file path and returns one instance of an ITcpCommandDictionary</param>
    /// <param name="commandsetPath">Path to the XML file containing the command set.</param>
    public ZusiTcpClientConnection(string clientId, ClientPriority priority, ITcpCommandDictionaryFactory dictionaryFactory,
      IDataReceptionHandlerFactory handlerFactory, INetworkIOHandlerFactory networkHandlerFactory, ITypedAndGenericEventManager<int> eventManager,
      IEventMarshalFactory marshalFactory, string commandsetPath = "commandset.xml") :
      this(clientId, priority, dictionaryFactory.Create(commandsetPath), handlerFactory, networkHandlerFactory, eventManager, marshalFactory)
    {
    }


    /// <summary>
    /// Initializes a new <see cref="ZusiTcpClientConnection"/> object that uses the specified event handlers to pass datasets to the client application.
    /// </summary>
    /// <param name="clientId">Identifies the client to the server. Use your application's name for this.</param>
    /// <param name="priority">Client priority. Determines measurement update frequency. Recommended value for control desks: "High"</param>
    /// <param name="commands">A set of commands.</param>
    /// <param name="receptionHandlerFactoryOld">A delegate to a factory method that produces a DataReceptionHandlerOld using the
    /// synchronization context as parameter.</param>
    private ZusiTcpClientConnection(string clientId, ClientPriority priority, ITcpCommandDictionary commands,
      IDataReceptionHandlerFactory receptionHandlerFactory, INetworkIOHandlerFactory networkHandlerFactory, ITypedAndGenericEventManager<int> eventManager,
      IEventMarshalFactory marshalFactory)
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

      _dataReceptionHandler = receptionHandlerFactory.Create(marshalFactory.Create(_hostContext, eventManager), commands);

      _commands = commands;
      _networkHandlerFactory = networkHandlerFactory;
      _eventManager = eventManager;
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

    private void ExceptionMarshal(object exception)
    {
      var castException = exception as ZusiTcpException;

      Debug.Assert(castException != null);

      if (ErrorReceived == null)
      {
        return;
      }

      ErrorReceived.Invoke(this, new ErrorEventArgs(castException));
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

        _networkIOHandler = _networkHandlerFactory.Create(endPoint);
        _dataReceptionHandler.ClientReader = _networkIOHandler;

        _streamReaderThread = new Thread(ReceiveLoop) { Name = "ZusiData Receiver" };
        _streamReaderThread.IsBackground = true;

        new HandshakeHandler(_networkIOHandler).HandleHandshake(RequestedData, ClientPriority, ClientId);
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
      _networkIOHandler.Disconnect();
      _networkHandlerFactory.Close(_networkIOHandler);
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
        _hostContext.Post(ExceptionMarshal, e as ZusiTcpException);
      }
      else if (e is EndOfStreamException)
      {
        /* EndOfStream occurs when the NetworkStream reaches its end while the binaryReader tries to read from it.
         * This happens when the socket closes the stream.
         */
        var newEx = new ZusiTcpException("Connection to the TCP server has been lost.", e);
        _hostContext.Post(ExceptionMarshal, newEx);
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

        _hostContext.Post(ExceptionMarshal, newEx);
      }
    }

    private void ReceivePacket()
    {
      int packetLength = _networkIOHandler.ReadInt32();

      int curInstr = BitbangingHelpers.GetInstruction(_networkIOHandler.ReadByte(), _networkIOHandler.ReadByte());

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
      int curID = _networkIOHandler.ReadByte() + 256 * curInstr;

      // One byte read for curID
      int bytesRead = 1;

      bytesRead += _dataReceptionHandler.HandleData(curID);
      return bytesRead;
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

    public void RequestData<T>(string name, EventHandler<DataReceivedEventArgs<T>> eventHandler)
    {
      RequestData(IDs[name], eventHandler);
    }

    public void RequestData<T>(int id, EventHandler<DataReceivedEventArgs<T>> eventHandler)
    {
      RequestData(id);

      Subscribe(id, eventHandler);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
      {
        return;
      }

      if (_networkIOHandler != null)
      {
        _networkIOHandler.Disconnect();
        _networkIOHandler = null;
      }

      if (_streamReaderThread == null)
      {
        return;
      }

      _streamReaderThread.Abort();
      _streamReaderThread = null;
    }
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