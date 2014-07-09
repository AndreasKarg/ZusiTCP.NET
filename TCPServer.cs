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
  public class TCPServer
  {
    #region Fields

    private readonly List<TCPServerSlaveConnection> _clients = new List<TCPServerSlaveConnection>();
    private readonly List<Base_Connection> _clientsExtern = new List<Base_Connection>();
    private readonly ReadOnlyCollection<Base_Connection> _clientsExternReadonly;

    private Thread _accepterThread;
    private TCPCommands _doc;
    private TcpListener _socketListener;

    #endregion

    private readonly ReferenceCounter<int> _requestedData = new ReferenceCounter<int>();
    private TCPServerMasterConnection _masterL;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Zusi_Datenausgabe.TCPServer" /> class.
    /// </summary>
    /// <param name="commandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
    public TCPServer(TCPCommands commandsetDocument)
    {
      _clientsExternReadonly = _clientsExtern.AsReadOnly();
      _doc = commandsetDocument;
      _anywayReqReadonly = new System.Collections.ObjectModel.ReadOnlyCollection<int>(_anywayReq);
    }

    /// <summary>
    ///   Gets the master.
    /// </summary>
    /// <value>The master.</value>
    public Base_Connection Master
    {
      get { return _masterL; }
    }

    private System.Collections.Generic.List<int> _anywayReq = new System.Collections.Generic.List<int>();
    private System.Collections.ObjectModel.ReadOnlyCollection<int> _anywayReqReadonly;
    /// <summary>
    ///   Sets the given Ids to be requested from the master even if no client wants them.
    /// </summary>
    public void RepalceAnywayRequested(IEnumerable<int> newAnywayReq)
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
    ///   Gets the Clients.
    /// </summary>
    /// <value>A readonly List of all connected (or connecting) Clients.</value>
    public ReadOnlyCollection<Base_Connection> Clients
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
      ErrorEvent handler = OnError;
      if (handler != null)
      {
        handler(this, ex);
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
      foreach (var cli in _clients)
      {
        cli.Disconnect();
      }
      _accepterThread.Abort();
    }

    private void RunningLoop()
    {
      try
      {
        while (IsStarted)
        {
          TcpClient socketClient = _socketListener.AcceptTcpClient();

          //TODO: Cleanup Initializer class
          TCPServerConnectionInitializer initializer = new TCPServerConnectionInitializer("", ClientPriority.Undefined, _doc, null); //Not Syncronized
          initializer.MasterConnectionInitialized += MasterConnectionInitialized;
          initializer.SlaveConnectionInitialized += SlaveConnectionInitialized;
          initializer.InitializeClient(new BinaryIoTcpClient(socketClient, true));
        }
      }
      catch
      {
        _socketListener.Stop();
        throw;
      }
    }

    private void SlaveConnectionInitialized(object sender, EventArgs eventArgs)
    {
      TCPServerConnectionInitializer initializer = sender.AssertedCast<TCPServerConnectionInitializer>();

      //TODO: Throw exception in client when ErrorReceived is not subscribed to.

      //No Check if master is null any more due to the check if data is already requested.

      TCPServerSlaveConnection slave = initializer.SlaveConnection;
      slave.DataRequested += OnSlaveDataRequested;
      slave.DataChecking += OnSlaveDataChecking;
      slave.ConnectionState_Changed += SlaveConnectionStateChanged;
      slave.ErrorReceived += SlaveErrorReceived;
      _clients.Add(slave);
      _clientsExtern.Add(slave);
      slave.InitializeClient();
    }

    private void SlaveErrorReceived(object sender, ZusiTcpException zusiTcpException)
    {
      TCPServerSlaveConnection client = sender.AssertedCast<TCPServerSlaveConnection>();

      KillClient(client);
      InvokeOnError(new ZusiTcpException("A client generated an exception.", zusiTcpException));
    }

    private void KillClient(TCPServerSlaveConnection client)
    {
      Debug.Assert(_clients.Contains(client));
      Debug.Assert(client.RequestedData != null);

      if (!_clients.Contains(client)) return;
      _requestedData.ReleaseRange(client.RequestedData);
      _clients.Remove(client);
      _clientsExtern.Remove(client);
      client.Dispose();
    }

    private void SlaveConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      TCPServerSlaveConnection client = sender.AssertedCast<TCPServerSlaveConnection>();
      Debug.Assert(_clients.Contains(client));

      switch (client.ConnectionState)
      {
        case ConnectionState.Connected:
          if (_masterL != null)
          {
            foreach(var val in client.RequestedData)
            {
              byte[] data;
              if (_masterL.TryGetBufferValue(val, out data))
              {
                client.SendByteCommand(data, val);
              }
            }
          }
          break;
        case ConnectionState.Connecting:
          /* Nothing to do */
          break;
        case ConnectionState.Error:
        case ConnectionState.Disconnected:
          KillClient(client);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void OnSlaveDataChecking(object sender, EventArgs eventArgs)
    {
      TCPServerSlaveConnection client = sender.AssertedCast<TCPServerSlaveConnection>();
      if (_masterL != null)
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
      TCPServerSlaveConnection client = sender.AssertedCast<TCPServerSlaveConnection>();
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
      TCPServerConnectionInitializer initializer = sender.AssertedCast<TCPServerConnectionInitializer>();

      if (_masterL != null)
      {
        initializer.RefuseConnectionAndTerminate();
        throw new NotSupportedException("Master is already connected. Cannot accept more than one master.");
      }
      _masterL = initializer.GetMasterConnection(_requestedData.ReferencedToIEnumerable());
      _masterL.ConnectionState_Changed += MasterConnectionStateChanged;
      _masterL.ErrorReceived += MasterErrorReceived;
      _masterL.DataSetReceived += MasterDataSetReceived;
    }

    private void MasterDataSetReceived(DataSet<byte[]> dataSet)
    {
      HandleServerRelatedRequest(dataSet.Value, dataSet.Id);

      foreach (var client in _clients)
      {
        client.SendByteCommand(dataSet.Value, dataSet.Id);
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
