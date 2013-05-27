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
  [EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
  public class TCPServer
  {
    #region Fields

    private readonly List<TCPServerCleint> _clients = new List<TCPServerCleint>();
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
    private TCPServerCleint MasterL { get; set; }
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
    private void ConstByteCommandReceived(byte[] array, int id, TCPServerCleint sender)
    {
      ServerRelatedRequest(array, id);
      if (sender.ClientPriority == ClientPriority.Master)
      {
        foreach (TCPServerCleint cli in _clients)
        {
          if (cli != sender)
            cli.SendByteCommand(array, id);
        }
      }
    }
    private void LengthIn1ByteCommandReceived(byte[] array, int id, TCPServerCleint sender)
    {
      if (sender.ClientPriority == ClientPriority.Master)
      {
        foreach (TCPServerCleint cli in _clients)
        {
          if (cli != sender)
            cli.SendLengthIn1ByteCommand(array, id);
        }
      }
    }
    private void ConnectionConnectStatusChanged(TCPServerCleint sender)
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
      foreach (TCPServerCleint cli in _clients)
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
      foreach (TCPServerCleint cli in _clients)
      {
        cli.Disconnnect();
      }
      _accepterThread.Abort();
    }
    private void RunningLoop()
    {
      TCPServerCleint cli = null;
      try
      {
        while (IsStarted)
        {
          TcpClient tc = _serverObj.AcceptTcpClient();
          cli = new TCPServerCleint(_doc, this);

          cli.TryBeginAcceptConnection(tc, (Master != null) ? Master.RequestedData : null);
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

    private class TCPServerCleint : Base_Connection
    {
      public TCPServerCleint(TCPCommands commandsetDocument, TCPServer ServerBase)
        : base("Unknown", ClientPriority.Undefined, commandsetDocument, null)
      {
        this.ServerBase = ServerBase;
        this.ConnectionState_Changed += ConnectionConnectStatusChanged;
      }

      public TCPServer ServerBase { set; get; }

      protected override void HandleHandshake()
      {
      }


      protected int HandleDATA_4ByteCommand(BinaryReader input, int id)
      {
        ServerBase.ConstByteCommandReceived(input.ReadBytes(4), id, this);
        return 4;
      }
      protected int HandleDATA_8ByteCommand(BinaryReader input, int id)
      {
        ServerBase.ConstByteCommandReceived(input.ReadBytes(8), id, this);
        return 8;
      }
      protected int HandleDATA_LengthIn1ByteCommand(BinaryReader input, int id)
      {
        byte lng = input.ReadByte();
        ServerBase.LengthIn1ByteCommandReceived(input.ReadBytes(lng), id, this);
        return lng + 1;
      }

      public void SendByteCommand(byte[] array, int id)
      {
        if ((ConnectionState != ConnectionState.Connected) || (!RequestedData.Contains(id)))
          return;
        List<byte> ida = new List<byte>(System.BitConverter.GetBytes(id));
        ida.RemoveAt(3);
        ida.Reverse();
        SendPacket(ida.ToArray(), array);
      }
      public void SendLengthIn1ByteCommand(byte[] array, int id)
      {
        if ((ConnectionState != ConnectionState.Connected) || (!RequestedData.Contains(id)))
          return;
        List<byte> ida = new List<byte>(System.BitConverter.GetBytes(id));
        ida.RemoveAt(3);
        ida.Reverse();
        byte[] lng = { (byte)array.Length };
        SendPacket(ida.ToArray(), lng, array);
      }

      protected override void TryBeginAcceptConnection_IsMaster()
      {
        this.RequestedData.Clear();
        this.RequestedData.AddRange(ServerBase.GetAbonentedIds());
        base.TryBeginAcceptConnection_IsMaster();
      }

      private void ConnectionConnectStatusChanged(object sender, EventArgs e)
      {
        ServerBase.ConnectionConnectStatusChanged(this);
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
      public void TryBeginAcceptConnection(TcpClient clientConnection, ICollection<int> RestrictToValues)
      {
        InitializeClient(clientConnection);
      }

      private void ReceiveLoop(object o)
      {
        System.Collections.ObjectModel.ReadOnlyCollection<int> RestrictToValues = (System.Collections.ObjectModel.ReadOnlyCollection<int>)o;
        try
        {
          RequestedData.Clear();
          try
          { ExpectResponse(ResponseType.Hello, 0); }
          catch
          { SendPacket(Pack(0, 2, 255)); throw; }
          if (this.ClientPriority == ClientPriority.Master)
          {
            if (RestrictToValues != null)
            {
              SendPacket(Pack(0, 2, 2));
              throw new ZusiTcpException("Master is already connected. No more Master connections allowed.");
            }
            try
            { TryBeginAcceptConnection_IsMaster(); }
            catch
            { SendPacket(Pack(0, 2, 255)); throw; }
            SendPacket(Pack(0, 2, 0));
            Connect_SendRequests();
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
                if ((RestrictToValues != null) && (!RestrictToValues.Contains(ValReq)))
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
        ReceiveLoop();
      }
    }
  }
}

