using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Zusi_Datenausgabe
{
  [EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
  public class TCPServer
  {
    #region Fields

    private readonly List<TCPServerClient> _clients = new List<TCPServerClient>();
    private readonly List<Base_Connection> _clients_extern = new List<Base_Connection>();
    private readonly System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection> _clients_extern_readonly;

    private TcpListener _serverObj = null;
    private Thread _accepterThread = null;
    private TCPCommands _doc;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Zusi_Datenausgabe.TCPServer"/> class.
    /// </summary>
    /// <param name="CommandsetDocument">The commandset document. Valid entrys for the types are 4ByteCommand, 8ByteCommand and LengthIn1ByteCommand.</param>
    public TCPServer(TCPCommands CommandsetDocument)
    {
      _clients_extern_readonly = new System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection>(_clients_extern);
      _doc = CommandsetDocument;
    }
    /// <summary>
    /// Gets a list of all connected clients.
    /// </summary>
    /// <value>The clients.</value>
    public System.Collections.ObjectModel.ReadOnlyCollection<Base_Connection> Clients { get { return _clients_extern_readonly; } }
    private TCPServerClient MasterL { get; set; }
    /// <summary>
    /// Gets the master.
    /// </summary>
    /// <value>The master.</value>
    public Base_Connection Master { get { return MasterL; } }
    private void ServerRelatedRequest(byte[] array, int id)
    {
      if ((id < 3840) || (id >= 4096))
        return;
      //ToDo: Code einfügen.
    }

    private IEnumerable<int> GetAbonentedIds()
    {
      List<int> lst = new List<int>();
      //Hier kann lst mit MUSS-ABONIEREN-Werten initialisiert werden, falls solcher gewünscht sind.
      foreach (TCPServerClient cli in _clients)
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
        _serverObj = new TcpListener(IPAddress.Any, port);
        _serverObj.Start();
        _accepterThread.Start();
      }
      catch
      {
        _accepterThread = null;
        if (_serverObj != null)
          _serverObj.Stop();
        _serverObj = null;
        throw;
      }

    }

    /// <summary>
    /// Stops the Server and closes all connected clients.
    /// </summary>
    public void Stop()
    {
      foreach (TCPServerClient cli in _clients)
      {
        cli.Disconnect();
      }
      _accepterThread.Abort();
    }
    private void RunningLoop()
    {
      TCPServerClient cli = null;
      try
      {
        while (IsStarted)
        {
          TcpClient tc = _serverObj.AcceptTcpClient();
          cli = new TCPServerClient(_doc, (Master != null) ? Master.RequestedData : null, GetAbonentedIds);

          cli.ConstByteCommandReceived += CLIOnConstByteCommandReceived;
          cli.LengthIn1ByteCommandReceived += OnLengthIn1ByteCommandReceived;
          cli.ConnectionState_Changed += OnConnectionStateChanged;

          cli.TryBeginAcceptConnection(tc);
          cli = null;
        }
      }
      catch
      {
        if (cli != null)
          cli.Dispose();
        _serverObj.Stop();
        throw;
      }
    }

    private void OnConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      Debug.Assert(sender is TCPServerClient);

      var serverClient = sender as TCPServerClient;

      if (serverClient.ConnectionState == ConnectionState.Connected)
      {
        if (serverClient.ClientPriority == ClientPriority.Master)
          MasterL = serverClient;
        if (!_clients.Contains(serverClient))
          _clients.Add(serverClient);
        if (!_clients_extern.Contains(serverClient))
          _clients_extern.Add(serverClient);
      }
      else
      {
        if (serverClient == MasterL)
          MasterL = null;
        _clients.Remove(serverClient);
        _clients_extern.Remove(serverClient);
        if (serverClient.ConnectionState == ConnectionState.Error)
        {
          serverClient.Disconnect();
          serverClient.Dispose();
        }
      }
    }

    private void OnLengthIn1ByteCommandReceived(object sender, CommandReceivedDelegateArgs args)
    {
      Debug.Assert(sender is TCPServerClient);
      var serverClient = sender as TCPServerClient;

      if (serverClient.ClientPriority != ClientPriority.Master) return;

      foreach (var client in _clients.Where(cli => cli != serverClient))
      {
        client.SendLengthIn1ByteCommand(args.Array, args.ID);
      }
    }

    private void CLIOnConstByteCommandReceived(object sender, CommandReceivedDelegateArgs args)
    {
      Debug.Assert(sender is TCPServerClient);

      var array = args.Array;
      var id = args.ID;

      var serverClient = sender as TCPServerClient;
      ServerRelatedRequest(array, id);

      if (serverClient.ClientPriority != ClientPriority.Master) return;

      foreach (var client in _clients.Where(cli => cli != serverClient))
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

