using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Zusi_Datenausgabe
{
  [EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
  public class TCPServer
  {
    #region Fields

    private readonly List<TCPServerSlaveConnection> _clients = new List<TCPServerSlaveConnection>();
    private readonly ReadOnlyCollection<TCPServerSlaveConnection> _clientsExternReadonly;

    private TcpListener _socketListener;
    private Thread _accepterThread;
    private TCPCommands _doc;

    #endregion

    public event ErrorEvent OnError;

    public void InvokeOnError(ZusiTcpException ex)
    {
      var handler = OnError;
      if (handler != null) handler(this, ex);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Zusi_Datenausgabe.TCPServer"/> class.
    /// </summary>
    /// <param name="commandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
    public TCPServer(TCPCommands commandsetDocument)
    {
      _clientsExternReadonly = _clients.AsReadOnly();
      _doc = commandsetDocument;
    }
/*
    /// <summary>
    /// Gets a list of all connected clients.
    /// </summary>
    /// <value>The clients.</value>
    public ReadOnlyCollection<Base_Connection> Clients { get { return _clientsExternReadonly; } }
*/

    private TCPServerMasterConnection _masterL;
    private ICollection<int> _requestedData;

    /// <summary>
    /// Gets the master.
    /// </summary>
    /// <value>The master.</value>
    public Base_Connection Master { get { return _masterL; } }

    private void HandleServerRelatedRequest(byte[] array, int id)
    {
      if ((id < 3840) || (id >= 4096))
        return;
      //ToDo: Code einfügen.
    }

    public bool IsStarted { get { return _accepterThread != null; } }

    /// <summary>
    /// Starts the server using the specified port.
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
          _socketListener.Stop();
        _socketListener = null;
        throw;
      }

    }

    /// <summary>
    /// Stops the Server and closes all connected clients.
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
          var initializer = new TCPServerConnectionInitializer("", ClientPriority.Undefined);
          initializer.MasterConnectionInitialized += MasterConnectionInitialized;
          initializer.SlaveConnectionInitialized += SlaveConnectionInitialized;
          initializer.InitializeClient(socketClient);
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
      var initializer = sender.AssertedCast<TCPServerConnectionInitializer>();

      //TODO: Throw exception in client when ErrorReceived is not subscribed to.

      //TODO: Improve slave handling.
      if(_masterL != null)
      {
        initializer.RefuseConnectionAndTerminate();
        throw new NotSupportedException("Master is already connected. Cannot accept more clients.");
      }

      var slave = initializer.SlaveConnection;
      slave.DataRequested += OnSlaveDataRequested;
      slave.ConnectionState_Changed += SlaveConnectionStateChanged;
      slave.ErrorReceived += SlaveErrorReceived;
      _clients.Add(slave);
    }

    private void SlaveErrorReceived(object sender, ZusiTcpException zusiTcpException)
    {
      var client = sender.AssertedCast<TCPServerSlaveConnection>();

      KillClient(client);
      InvokeOnError(new ZusiTcpException("A client generated an exception.", zusiTcpException));
    }

    private void KillClient(TCPServerSlaveConnection client)
    {
      Debug.Assert(_clients.Contains(client));
      client.Dispose();
      _clients.Remove(client);
    }

    private void SlaveConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      var client = sender.AssertedCast<TCPServerSlaveConnection>();
      Debug.Assert(_clients.Contains(client));

      switch (client.ConnectionState)
      {
        case ConnectionState.Connected:
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

    private void OnSlaveDataRequested(object sender, EventArgs eventArgs)
    {
      var client = sender.AssertedCast<TCPServerSlaveConnection>();
      Debug.Assert(_clients.Contains(client));

      //TODO: Implement manager for requested data.
      throw new NotImplementedException();
    }

    private void MasterConnectionInitialized(object sender, EventArgs e)
    {
      var initializer = sender.AssertedCast<TCPServerConnectionInitializer>();

      if (_masterL != null)
      {
        initializer.RefuseConnectionAndTerminate();
        throw new NotSupportedException("Master is already connected. Cannot accept more than one master.");
      }

      _masterL = initializer.GetMasterConnection(_requestedData);
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
      Debug.Assert(_masterL != null);
      _masterL.Dispose();
      _masterL = null;
    }

    private void MasterConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      Debug.Assert(sender == _masterL);

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

  internal delegate IEnumerable<int> GetAbonentedIdsDelegate();

  internal class CommandReceivedDelegateArgs : EventArgs
  {
    private readonly byte[] _array;
    private readonly int _id;

    public byte[] Array
    {
      [DebuggerStepThrough]
      get { return _array; }
    }

    public int ID
    {
      [DebuggerStepThrough]
      get { return _id; }
    }

    public CommandReceivedDelegateArgs(byte[] array, int id)
    {
      _array = array;
      _id = id;
    }
  }
}

