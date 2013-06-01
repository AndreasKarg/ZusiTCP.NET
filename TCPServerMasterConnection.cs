using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Zusi_Datenausgabe
{
  internal class TCPServerMasterConnection : Base_Connection
  {
    public event Action<DataSet<byte[]>> DataSetReceived;

    private void OnDataSetReceived(DataSet<byte[]> args)
    {
      var handler = DataSetReceived;
      if (handler != null) handler(args);
    }

    private void OnDataSetReceived(int id, byte[] payload)
    {
      OnDataSetReceived(new DataSet<byte[]>(id, payload));
    }

    private GetAbonentedIdsDelegate _getAbonentedIds;

    public TCPServerMasterConnection(SynchronizationContext hostContext, TcpClient client, String clientId, GetAbonentedIdsDelegate getAbonentedIds)
      : base(clientId, ClientPriority.Master, (TCPCommands)null, hostContext)
    {
      _getAbonentedIds = getAbonentedIds;
      InitializeClient(client);
    }

    protected override void HandleHandshake()
    {
      try
      {
        TryBeginAcceptConnectionIsMaster();
      }
      catch
      {
        SendPacket(Pack(0, 2, 255));
        throw;
      }
      SendPacket(Pack(0, 2, 0));
      RequestDataFromZusi();
    }

    private void TryBeginAcceptConnectionIsMaster()
    {
      RequestedData.Clear();
      RequestedData.AddRange(_getAbonentedIds());
    }

    protected override void ReceiveLoop()
    {
      DefaultReceiveLoop();
    }

    protected int HandleDATA_4ByteCommand(BinaryReader input, int id)
    {
      OnDataSetReceived(id, input.ReadBytes(4));
      return 4;
    }
    protected int HandleDATA_8ByteCommand(BinaryReader input, int id)
    {
      OnDataSetReceived(id, input.ReadBytes(8));
      return 8;
    }

    protected int HandleDATA_LengthIn1ByteCommand(BinaryReader input, int id)
    {
      var lng = input.ReadByte();

      var result = new byte[lng+1];

      result[0] = lng;
      input.Read(result, 1, lng);

      OnDataSetReceived(id, result);
      return lng + 1;
    }
  }
}