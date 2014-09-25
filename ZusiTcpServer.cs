#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  ///<summary>This class represents a TCP Server that connects multiple slaves 
  /// (slaves can be implented using class ZusiTcpCon) with a master (normally Zusi).
  /// Slaves normally should be connected before the master is connected, but can also be connected 
  /// after the master when no new data is required. Otherwise the client will be denyed.</summary>
  ///<remarks>Warning: This class lacks documentation. But method names should do it.</remarks>
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  public class ZusiTcpServer
  {
    #region Fields

    private readonly List<ZusiTcpServerSlaveConnection> _clients = new List<ZusiTcpServerSlaveConnection>();
    private readonly List<ZusiTcpBaseConnection> _clientsExtern = new List<ZusiTcpBaseConnection>();
    private readonly ReadOnlyCollection<ZusiTcpBaseConnection> _clientsExternReadonly;

    private Thread _accepterThread;
    private CommandSet _doc;
    private TcpListener _socketListener;

    private readonly SynchronizationContext HostContext;

    #endregion

    private readonly ReferenceCounter<int> _requestedData = new ReferenceCounter<int>();
    private ZusiTcpServerMasterConnection _masterL;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Zusi_Datenausgabe.ZusiTcpServer" /> class.
    /// </summary>
    /// <param name="commandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
    /// <param name="hostContext">A Context bring the Datas to the current Thread. Can be null for avoid syncronisation.</param>
    public ZusiTcpServer(CommandSet commandsetDocument, SynchronizationContext hostContext)
    {
      _clientsExternReadonly = _clientsExtern.AsReadOnly();
      _doc = commandsetDocument;
      _anywayReqReadonly = new System.Collections.ObjectModel.ReadOnlyCollection<int>(_anywayReq);
      HostContext = hostContext;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="Zusi_Datenausgabe.ZusiTcpServer" /> class.
    /// </summary>
    /// <param name="commandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
    /// <exception cref="ObjectUnsynchronisableException">Thrown, when SynchronizationContext.Current == null.</exception>
    public ZusiTcpServer(CommandSet commandsetDocument)
      : this(commandsetDocument, SynchronizationContext.Current)
    {
      if (SynchronizationContext.Current == null)
      {
        throw new ObjectUnsynchronisableException();
      }
    }

    /// <summary>
    ///   Gets the master.
    /// </summary>
    /// <value>The master.</value>
    public ZusiTcpBaseConnection Master
    {
      get { return _masterL; }
    }

    private System.Collections.Generic.List<int> _anywayReq = new System.Collections.Generic.List<int>();
    private System.Collections.ObjectModel.ReadOnlyCollection<int> _anywayReqReadonly;
    /// <summary>
    ///   Sets the given Ids to be requested from the master even if no client wants them.
    /// </summary>
    public void ReplaceAnywayRequested(IEnumerable<int> newAnywayReq)
    {
      if (_masterL != null)
        throw new System.InvalidOperationException();
      _requestedData.ReleaseRange(_anywayReq);
      _anywayReq.Clear();
      _anywayReq.AddRange(newAnywayReq);
      _requestedData.ClaimRange(_anywayReq);
    }
    /// <summary>
    ///   Gets a List of Ids that will be requested from the master even if no client wants them. 
    ///   Usefull for clients that want to connec after the master.
    /// </summary>
    /// <value>A readonly List of Ids that will be requested from the master even if no client wants them.</value>
    public System.Collections.ObjectModel.ReadOnlyCollection<int> AnywayRequested
    {
      get { return _anywayReqReadonly;}
    }


    /// <summary>
    ///   Gets the Clients. Warning: Disconnecting a Client changes this List immediately!
    /// </summary>
    /// <value>A readonly List of all connected (or connecting) Clients.</value>
    public ReadOnlyCollection<ZusiTcpBaseConnection> Clients
    {
      get { return _clientsExternReadonly;}
    }

    /// <summary>
    ///   Gets if the server is started.
    /// </summary>
    /// <value>True when the server is started, otherwise false.</value>
    public bool IsStarted
    {
      get { return _accepterThread != null; }
    }

    /// <summary>
    ///   Returns any Exeption thrown by any connected or connecting client. If this event occurs it does normally
    ///   NOT mean, that the server is about to close. Often some client is disconnecting or failed to connect.
    /// </summary>
    /// <remarks>Note: At the moment even a simple disconnect can cause this event to be raised. This may change in future.</remarks>
    public event ErrorEvent OnError;

    protected void InvokeOnError(ZusiTcpException ex) //ToDo: Why on earth was this method Public?
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
    private void ErrorMarshal(object o)
    {
      ZusiTcpException ex = (ZusiTcpException) o;
      ErrorEvent handler = OnError;
      if (handler != null)
      {
        handler(this, ex);
      }
    }

    /// <summary>
    ///   Raised, when someone connected to the server.
    /// </summary>
    public event ClientConnectedEvent ClientConnected;

    protected void InvokeClientConnected(ZusiTcpBaseConnection con)
    {
      if (HostContext != null)
      {
        HostContext.Post(ClientConnectedMarshal, con);
      }
      else
      {
        ClientConnectedMarshal(con);
      }
    }
    private void ClientConnectedMarshal(object o)
    {
      ZusiTcpBaseConnection con = (ZusiTcpBaseConnection) o;
      ClientConnectedEvent handler = ClientConnected;
      if (handler != null)
      {
        handler(this, con);
      }
    }

    private void HandleServerRelatedRequest(byte[] array, int id)
    {
      if ((id < 3840) || (id >= 4096))
      {
        return;
      }
      //ToDo: Code einfügen.
    }

    /// <summary>
    ///   Starts the server using the specified port.
    /// </summary>
    /// <param name="port">The port, the Server should use.</param>
    /// <exception cref="InvalidOperationException">Thrown, when the connection is already started.</exception>
    /// <exception cref="System.Net.Sockets.SocketException">Forwarded from System.Net.Sockets.TcpListener.Start(). 
    ///    Especially if another TCP-server is already started at this port.</exception>
    public void Start(int port)
    {
      if (IsStarted)
      {
        throw (new InvalidOperationException());
      }
      _accepterThread = new Thread(RunningLoop);
      try
      {
        _socketListener = new TcpListener(IPAddress.Any, port);
        _socketListener.Start();
        _accepterThread.Start();
      }
      catch
      {
        _accepterThread = null;
        if (_socketListener != null)
        {
          _socketListener.Stop();
        }
        _socketListener = null;
        throw;
      }
    }

    /// <summary>
    ///   Stops the Server and closes all connected clients.
    /// </summary>
    public void Stop()
    {
      var clientsOld = new System.Collections.Generic.List<ZusiTcpServerSlaveConnection>(_clients);
      foreach (var cli in clientsOld)
      {
        cli.Disconnect();
      }
      if (_masterL != null)
        _masterL.Disconnect();
      if (_accepterThread != null) //ToDo: Think about how the server should responde in this case.
        _accepterThread.Interrupt();
    }

    private void RunningLoop()
    {
      try
      {
        while (IsStarted)
        {
          while(!_socketListener.Pending()) { Thread.Sleep(250); } // While noone wants to connect - sleep!
          TcpClient socketClient = _socketListener.AcceptTcpClient();

          //TODO: Cleanup Initializer class
          ZusiTcpServerConnectionInitializer initializer = new ZusiTcpServerConnectionInitializer("", ClientPriority.Undefined, _doc, null); //Not Syncronized
          initializer.MasterConnectionInitialized += MasterConnectionInitialized;
          initializer.SlaveConnectionInitialized += SlaveConnectionInitialized;
          initializer.InitializeClient(new BinaryIoTcpClient(socketClient, true));
        }
      }
      catch(System.Threading.ThreadAbortException)
      {
        _socketListener.Stop();
        _socketListener = null;
        _accepterThread = null;
      }
      catch(System.Threading.ThreadInterruptedException)
      {
        _socketListener.Stop();
        _socketListener = null;
        _accepterThread = null;
      }
      catch(System.Exception ex)
      {
        _socketListener.Stop();
        _socketListener = null;
        _accepterThread = null;
        InvokeOnError(new ZusiTcpException("An Error occured while trying to find new clients.", ex));
      }
    }

    private void SlaveConnectionInitialized(object sender, EventArgs eventArgs)
    {
      ZusiTcpServerConnectionInitializer initializer = sender.AssertedCast<ZusiTcpServerConnectionInitializer>();

      //TODO: Throw exception in client when ErrorReceived is not subscribed to.

      //No Check if master is null any more due to the check if data is already requested.

      ZusiTcpServerSlaveConnection slave = initializer.GetSlaveConnection(HostContext);
      slave.DataRequested += OnSlaveDataRequested;
      slave.DataChecking += OnSlaveDataChecking;
      slave.ConnectionState_Changed += SlaveConnectionStateChanged;
      slave.ErrorReceived += SlaveErrorReceived;
      try
      {
        System.Threading.Monitor.Enter(_clients);
        _clients.Add(slave);
      }
      finally
      {
        System.Threading.Monitor.Exit(_clients);
      }
      try
      {
        System.Threading.Monitor.Enter(_clientsExtern);
        _clientsExtern.Add(slave);
      }
      finally
      {
        System.Threading.Monitor.Exit(_clientsExtern);
      }
      slave.InitializeClient();
      InvokeClientConnected(slave);
    }

    private void SlaveErrorReceived(object sender, ZusiTcpException zusiTcpException)
    {
      ZusiTcpServerSlaveConnection client = sender.AssertedCast<ZusiTcpServerSlaveConnection>();

      //KillSlave(client); //Not needed. Disconnect when this causes an ConnectionState Error.
      InvokeOnError(new ZusiTcpException("A client generated an exception.", zusiTcpException));
    }

    private void KillSlave(ZusiTcpServerSlaveConnection client)
    {
      Debug.Assert(_clients.Contains(client));
      Debug.Assert(client.RequestedData != null);

      if (!_clients.Contains(client)) return;
      _requestedData.ReleaseRange(client.RequestedData);
      try
      {
        System.Threading.Monitor.Enter(_clients);
        _clients.Remove(client);
      }
      finally
      {
        System.Threading.Monitor.Exit(_clients);
      }
      try
      {
        System.Threading.Monitor.Enter(_clientsExtern);
        _clientsExtern.Remove(client);
      }
      finally
      {
        System.Threading.Monitor.Exit(_clientsExtern);
      }
      client.Dispose();
    }

    private void SlaveConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      ZusiTcpServerSlaveConnection client = sender.AssertedCast<ZusiTcpServerSlaveConnection>();
      Debug.Assert(_clients.Contains(client));

      switch (client.ConnectionState)
      {
        case ConnectionState.Connected:
          if (_masterL != null)
          {
            //System.Threading.Monitor.Enter(_masterL.RequestedDataLockObject); //Access from multiple Threads. Lock
            foreach(var val in client.RequestedData)
            {
              byte[] data;
              if (_masterL.TryGetBufferValue(val, out data))
              {
                client.DataUpdate(data, val);
              }
            }
            //System.Threading.Monitor.Exit(_masterL.RequestedDataLockObject);
          }
          break;
        case ConnectionState.Connecting:
          /* Nothing to do */
          break;
        case ConnectionState.Error:
          client.Disconnect();
          break;
        case ConnectionState.Disconnected:
          KillSlave(client);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void OnSlaveDataChecking(object sender, EventArgs eventArgs)
    {
      ZusiTcpServerSlaveConnection client = sender.AssertedCast<ZusiTcpServerSlaveConnection>();
      foreach(var val in client.RequestedData) //Checks if all Data exist.
      {
        if (!_doc.ContainsID(val))
          throw new System.Collections.Generic.KeyNotFoundException(string.Format("ID {0} is unknown. " +
                                       "Cannot accept unknown data.", val));
      }
      if (_masterL != null) //Checks if all is requested from the master.
      {
        foreach(var val in client.RequestedData)
        {
          if (!_masterL.RequestedData.Contains(val))
            throw new NotSupportedException(string.Format("Master is already connected. Cannot accept Data {0}, " + 
                                                          "because it's not already requested.", val));
        }
      }
    }

    private void OnSlaveDataRequested(object sender, EventArgs eventArgs)
    {
      ZusiTcpServerSlaveConnection client = sender.AssertedCast<ZusiTcpServerSlaveConnection>();
      Debug.Assert(_clients.Contains(client));

      if (_masterL != null)
      {
        foreach(var val in client.RequestedData)
        {
          if (!_masterL.RequestedData.Contains(val))
            throw new NotSupportedException(string.Format("Master is already connected. Cannot accept Data {0}, " + 
                                                          "because it's not already requested.", val));
        }
      }
      _requestedData.ClaimRange(client.RequestedData);
    }

    private void MasterConnectionInitialized(object sender, EventArgs e)
    {
      ZusiTcpServerConnectionInitializer initializer = sender.AssertedCast<ZusiTcpServerConnectionInitializer>();

      if (_masterL != null)
      {
        initializer.RefuseConnectionAndTerminate();
        throw new NotSupportedException("Master is already connected. Cannot accept more than one master.");
      }
      _masterL = initializer.GetMasterConnection(HostContext, _requestedData.ReferencedToIEnumerable());
      _masterL.ConnectionState_Changed += MasterConnectionStateChanged;
      _masterL.ErrorReceived += MasterErrorReceived;
      _masterL.DataSetReceived += MasterDataSetReceived;
      InvokeClientConnected(_masterL);
    }

    private void MasterDataSetReceived(DataSet<byte[]> dataSet)
    {
      HandleServerRelatedRequest(dataSet.Value, dataSet.Id);

      foreach (var client in _clients)
      {
        client.DataUpdate(dataSet.Value, dataSet.Id);
      }
    }

    private void MasterErrorReceived(object sender, ZusiTcpException zusiTcpException)
    {
      KillMaster();

      InvokeOnError(new ZusiTcpException("The master generated an exception.", zusiTcpException));
    }

    private void KillMaster()
    {
      //Debug.Assert(_masterL != null);
      if (_masterL != null)
      {
        _masterL.Dispose();
        _masterL = null;
      }
    }

    private void MasterConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      if (_masterL == null) return;
      //Debug.Assert(sender == _masterL);

      switch (_masterL.ConnectionState)
      {
        case ConnectionState.Disconnected:
        case ConnectionState.Error:
          KillMaster();
          break;
        case ConnectionState.Connected:
        case ConnectionState.Connecting:
          // Nothing to do.
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
