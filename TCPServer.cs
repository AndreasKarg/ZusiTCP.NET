using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
    private readonly List<Base_Connection> _clientsExtern = new List<Base_Connection>();
    private readonly System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection> _clientsExternReadonly;

    private TcpListener _socketListener;
    private Thread _accepterThread;
    private TCPCommands _doc;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Zusi_Datenausgabe.TCPServer"/> class.
    /// </summary>
    /// <param name="commandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
    public TCPServer(TCPCommands commandsetDocument)
    {
      _clientsExternReadonly = new System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection>(_clientsExtern);
      _doc = commandsetDocument;
    }
    /// <summary>
    /// Gets a list of all connected clients.
    /// </summary>
    /// <value>The clients.</value>
    public System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection> Clients { get { return _clientsExternReadonly; } }

    private TCPServerMasterConnection _masterL;

    /// <summary>
    /// Gets the master.
    /// </summary>
    /// <value>The master.</value>
    public Base_Connection Master { get { return _masterL; } }

    private void ServerRelatedRequest(byte[] array, int id)
    {
      if ((id < 3840) || (id >= 4096))
        return;
      //ToDo: Code einfügen.
    }

    private IEnumerable<int> GetAbonentedIds()
    {
      var lst = new List<int>();
      //Hier kann lst mit MUSS-ABONIEREN-Werten initialisiert werden, falls solcher gewünscht sind.
      foreach (var cli in _clients)
      {
        foreach (int dat in cli.RequestedData)
        {
          if ((!lst.Contains(dat)) && ((dat < 3840) || (dat >= 4096)))
            lst.Add(dat);
        }
      }
      return lst;
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
      throw new NotImplementedException();
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
          client.Dispose();
          _clients.Remove(client);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void OnSlaveDataRequested(object sender, EventArgs eventArgs)
    {
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

      _masterL = initializer.GetMasterConnection(GetAbonentedIds);
      _masterL.ConnectionState_Changed += MasterConnectionStateChanged;
      _masterL.ErrorReceived += MasterErrorReceived;
      _masterL.DataSetReceived += MasterDataSetReceived;
    }

    private void MasterDataSetReceived(DataSet<byte[]> dataSet)
    {
      CLIOnConstByteCommandReceived(new CommandReceivedDelegateArgs(dataSet.Value, dataSet.Id));
    }

    private void MasterErrorReceived(object sender, ZusiTcpException zusiTcpException)
    {
      throw new NotImplementedException();
    }

    private void MasterConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      Debug.Assert(sender == _masterL);

      switch (_masterL.ConnectionState)
      {
        case ConnectionState.Disconnected:
        case ConnectionState.Error:
          _masterL.Dispose();
          _masterL = null;
          break;
        case ConnectionState.Connected:
        case ConnectionState.Connecting:
          // Nothing to do.
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      throw new NotImplementedException();
    }

    private void CLIOnConstByteCommandReceived(CommandReceivedDelegateArgs args)
    {
      var array = args.Array;
      var id = args.ID;

      ServerRelatedRequest(array, id);

      foreach (var client in _clients)
      {
        client.SendByteCommand(array, id);
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

