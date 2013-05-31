using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private void ConstByteCommandReceived(byte[] array, int id, TCPServerClient sender)
    {
      ServerRelatedRequest(array, id);
      if (sender.ClientPriority == ClientPriority.Master)
      {
        foreach (TCPServerClient cli in _clients)
        {
          if (cli != sender)
            cli.SendByteCommand(array, id);
        }
      }
    }
    private void LengthIn1ByteCommandReceived(byte[] array, int id, TCPServerClient sender)
    {
      if (sender.ClientPriority == ClientPriority.Master)
      {
        foreach (TCPServerClient cli in _clients)
        {
          if (cli != sender)
            cli.SendLengthIn1ByteCommand(array, id);
        }
      }
    }
    private void ConnectionConnectStatusChanged(TCPServerClient sender)
    {
      if (sender.ConnectionState == ConnectionState.Connected)
      {
        if (sender.ClientPriority == ClientPriority.Master)
          this.MasterL = sender;
        if (!_clients.Contains(sender))
          _clients.Add(sender);
        if (!_clients_extern.Contains(sender))
          _clients_extern.Add(sender);
      }
      else
      {
        if (sender == this.MasterL)
          MasterL = null;
        _clients.Remove(sender);
        _clients_extern.Remove(sender);
        if (sender.ConnectionState == ConnectionState.Error)
        {
          sender.Disconnnect();
          sender.Dispose();
        }
      }
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
        cli.Disconnnect();
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
          cli.LengthIn1ByteCommandReceived += CLIOnLengthIn1ByteCommandReceived;
          cli.ConnectionState_Changed += CLIOnConnectionStateChanged;

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

    private void CLIOnConnectionStateChanged(object sender, EventArgs eventArgs)
    {
      Debug.Assert(sender is TCPServerClient);
      ConnectionConnectStatusChanged(sender as TCPServerClient);
    }

    private void CLIOnLengthIn1ByteCommandReceived(object sender, CommandReceivedDelegateArgs args)
    {
      Debug.Assert(sender is TCPServerClient);
      LengthIn1ByteCommandReceived(args.Array, args.ID, sender as TCPServerClient);
    }

    private void CLIOnConstByteCommandReceived(object sender, CommandReceivedDelegateArgs args)
    {
      Debug.Assert(sender is TCPServerClient);
      ConstByteCommandReceived(args.Array, args.ID, sender as TCPServerClient);
    }
  }

  internal class TCPServerClient : Base_Connection
  {
    private ICollection<int> _restrictToValues;

    private GetAbonentedIdsDelegate _getAbonentedIds;

    public TCPServerClient(TCPCommands commandsetDocument, ICollection<int> restrictToValues, GetAbonentedIdsDelegate getAbonentedIds)
      : base("Unknown", ClientPriority.Undefined, commandsetDocument, null)
    {
      _restrictToValues = restrictToValues;
      _getAbonentedIds = getAbonentedIds;
    }

    protected override void HandleHandshake()
    {
    }

    public event EventHandler<CommandReceivedDelegateArgs> ConstByteCommandReceived;
    public event EventHandler<CommandReceivedDelegateArgs> LengthIn1ByteCommandReceived;

    public void OnLengthIn1ByteCommandReceived(CommandReceivedDelegateArgs args)
    {
      var handler = LengthIn1ByteCommandReceived;
      if (handler != null) handler(this, args);
    }

    public void OnLengthIn1ByteCommandReceived(byte[] array, int id)
    {
      OnLengthIn1ByteCommandReceived(new CommandReceivedDelegateArgs(array, id));
    }

    public void OnConstByteCommandReceived(CommandReceivedDelegateArgs args)
    {
      var handler = ConstByteCommandReceived;
      if (handler != null) handler(this, args);
    }

    public void OnConstByteCommandReceived(byte[] array, int id)
    {
      OnConstByteCommandReceived(new CommandReceivedDelegateArgs(array, id));
    }

    protected int HandleDATA_4ByteCommand(BinaryReader input, int id)
    {
      OnConstByteCommandReceived(input.ReadBytes(4), id);
      return 4;
    }
    protected int HandleDATA_8ByteCommand(BinaryReader input, int id)
    {
      OnConstByteCommandReceived(input.ReadBytes(8), id);
      return 8;
    }

    protected int HandleDATA_LengthIn1ByteCommand(BinaryReader input, int id)
    {
      byte lng = input.ReadByte();
      OnLengthIn1ByteCommandReceived(input.ReadBytes(lng), id);
      return lng + 1;
    }

    public void SendByteCommand(byte[] array, int id)
    {
      if ((ConnectionState != ConnectionState.Connected) || (!RequestedData.Contains(id)))
        return;
      List<byte> ida = new List<byte>(BitConverter.GetBytes(id));
      ida.RemoveAt(3);
      ida.Reverse();
      SendLargePacket(ida.ToArray(), array);
    }
    public void SendLengthIn1ByteCommand(byte[] array, int id)
    {
      if ((ConnectionState != ConnectionState.Connected) || (!RequestedData.Contains(id)))
        return;
      List<byte> ida = new List<byte>(BitConverter.GetBytes(id));
      ida.RemoveAt(3);
      ida.Reverse();
      byte[] lng = { (byte)array.Length };
      SendLargePacket(ida.ToArray(), lng, array);
    }

    protected virtual void TryBeginAcceptConnection_IsMaster()
    {
      this.RequestedData.Clear();
      this.RequestedData.AddRange(_getAbonentedIds());
    }

    /// <summary>
    /// Establish a connection to the TCP client as a server.
    /// </summary>
    /// <param name="clientConnection">The Client to Connect.</param>
    /// <param name="RequestedToZusi">A List to restrict ID-Requests. If a requested the value is not in the list, the
    /// connection will be terminated with error code 3. If the list is null all ID-Requests will be accepted. Note:
    /// when this param is set, masters will be denyed with code 2 - master already connected.</param>
    /// <exception cref="ZusiTcpException">This exception is thrown when the connection could not be
    /// established.</exception>
    public void TryBeginAcceptConnection(TcpClient clientConnection)
    {
      InitializeClient(clientConnection);
    }

    protected override void ReceiveLoop()
    {
      try
      {
        RequestedData.Clear();
        try
        { ExpectResponse(ResponseType.Hello, 0); }
        catch
        { SendPacket(Pack(0, 2, 255)); throw; }
        if (this.ClientPriority == ClientPriority.Master)
        {
          if (_restrictToValues != null)
          {
            SendPacket(Pack(0, 2, 2));
            throw new ZusiTcpException("Master is already connected. No more Master connections allowed.");
          }
          try
          { TryBeginAcceptConnection_IsMaster(); }
          catch
          { SendPacket(Pack(0, 2, 255)); throw; }
          SendPacket(Pack(0, 2, 0));
          RequestDataFromZusi();
        }
        else
        {
          SendPacket(Pack(0, 2, 0));
          ExpectResponseAnswer requestedValues = null;
          int dataGroup = -1;
          while ((requestedValues == null) || (requestedValues.requestArea != 0) || ((requestedValues.requestArray != null) && (requestedValues.requestArray.Length != 0)))
          {
            try
            { requestedValues = ExpectResponse(ResponseType.NeededData, dataGroup); }
            catch
            { SendPacket(Pack(0, 4, 255)); throw; }
            foreach (int ValReq in requestedValues.requestArray)
            {
              if ((_restrictToValues != null) && (!_restrictToValues.Contains(ValReq)))
              {
                SendPacket(Pack(0, 4, 3));
                throw new ZusiTcpException("Client requested dataset " + ValReq + " which was not requested when Zusi was connected.");
              }
              if (!RequestedData.Contains(ValReq))
                RequestedData.Add(ValReq);
            }
            SendPacket(Pack(0, 4, 0));
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
        else
        {
          var newEx =
            new ZusiTcpException("The connection can't be established.", e);

          PostExToHost(newEx);
        }
      }
      ConnectionState = ConnectionState.Connected;
      base.ReceiveLoop();
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

