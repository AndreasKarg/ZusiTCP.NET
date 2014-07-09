#region Using

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  internal class TCPServerMasterConnection : ZusiTcpReceiver
  {
    private readonly IEnumerable<int> _requestedIds;

    public TCPServerMasterConnection(SynchronizationContext hostContext,
                                     IBinaryIO client,
                                     String clientId,
                                     IEnumerable<int> requestedIds,
                                     TCPCommands commands)
      : base(clientId, ClientPriority.Master, hostContext, commands)
    {
      _requestedIds = requestedIds;
      InitializeClient(client);
    }

    public event Action<DataSet<byte[]>> DataSetReceived;

    private void OnDataSetReceived(DataSet<byte[]> args)
    {
      var handler = DataSetReceived;
      if (handler != null)
      {
        handler(args);
      }
    }

    private void OnDataSetReceived(int id, byte[] payload)
    {
      OnDataSetReceived(new DataSet<byte[]>(id, payload));
    }

    protected override void HandleHandshake()
    {
      SendPacket(Pack(0, 2, 0));
      RequestDataFromZusi(_requestedIds);
    }

    protected override void ReceiveLoop()
    {
      DefaultReceiveLoop();
    }

    protected int HandleDATA_4ByteCommand(IBinaryReader input, int id)
    {
      OnDataSetReceived(id, input.ReadBytes(4));
      return 4;
    }

    protected int HandleDATA_8ByteCommand(IBinaryReader input, int id)
    {
      OnDataSetReceived(id, input.ReadBytes(8));
      return 8;
    }

    protected int HandleDATA_LengthIn1ByteCommand(IBinaryReader input, int id)
    {
      byte lng = input.ReadByte();

      var result = new byte[lng + 1];

      result[0] = lng;
      input.Read(result, 1, lng);

      OnDataSetReceived(id, result);
      return lng + 1;
    }
  }
}
