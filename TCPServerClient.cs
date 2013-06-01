using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Zusi_Datenausgabe
{
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
      RequestedData.Clear();
      try
      {
        ExpectResponse(ResponseType.Hello, 0);
      }
      catch
      {
        SendPacket(Pack(0, 2, 255));
        throw;
      }
      if (this.ClientPriority == ClientPriority.Master)
      {
        if (_restrictToValues != null)
        {
          SendPacket(Pack(0, 2, 2));
          throw new ZusiTcpException("Master is already connected. No more Master connections allowed.");
        }
        try
        {
          TryBeginAcceptConnection_IsMaster();
        }
        catch
        {
          SendPacket(Pack(0, 2, 255));
          throw;
        }
        SendPacket(Pack(0, 2, 0));
        RequestDataFromZusi();
      }
      else
      {
        SendPacket(Pack(0, 2, 0));
        ExpectResponseAnswer requestedValues = null;
        int dataGroup = -1;
        while ((requestedValues == null) || (requestedValues.RequestedDataGroup != 0) ||
               ((requestedValues.RequestedValues != null) && (requestedValues.RequestedValues.Length != 0)))
        {
          try
          {
            requestedValues = ExpectResponse(ResponseType.NeededData, dataGroup);
          }
          catch
          {
            SendPacket(Pack(0, 4, 255));
            throw;
          }
          foreach (int ValReq in requestedValues.RequestedValues)
          {
            if ((_restrictToValues != null) && (!_restrictToValues.Contains(ValReq)))
            {
              SendPacket(Pack(0, 4, 3));
              throw new ZusiTcpException("Client requested dataset " + ValReq +
                                         " which was not requested when Zusi was connected.");
            }
            if (!RequestedData.Contains(ValReq))
              RequestedData.Add(ValReq);
          }
          SendPacket(Pack(0, 4, 0));
        }
      }
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
      DefaultReceiveLoop();
    }
  }
}