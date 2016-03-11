#region Using

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  internal class ZusiTcpServerMasterConnection : ZusiTcpClientAbstract
  {
    private readonly IList<int> _requestedIds;
    private System.Collections.ObjectModel.ReadOnlyCollection<int> _requestedData;
    public override ICollection<int> RequestedData { get { return _requestedData; } }
    //public object RequestedDataLockObject { get { return _dataBufferLock; } }

    private System.Collections.Generic.Dictionary<int, byte[]> _dataBuffer;
    //private object _dataBufferLock = new object();

    public ZusiTcpServerMasterConnection(SynchronizationContext hostContext,
                                     IBinaryIO client,
                                     String clientId,
                                     IList<int> requestedIds,
                                     CommandSet commands)
      : base(clientId, ClientPriority.Master, hostContext, commands)
    {
      _requestedIds = requestedIds;
      _requestedData = new System.Collections.ObjectModel.ReadOnlyCollection<int>(_requestedIds);
      _dataBuffer = new System.Collections.Generic.Dictionary<int, byte[]>();
      InitializeClient(client);
    }

    public event Action<DataSet<byte[]>> DataSetReceived;

    public bool TryGetBufferValue(int key, out byte[] value)
    {
       return _dataBuffer.TryGetValue(key, out value);
    }


    private void OnDataSetReceived(DataSet<byte[]> args)
    {
      var handler = DataSetReceived;
      if (handler != null)
      {
        handler(args);
      }
      //System.Threading.Monitor.Enter(RequestedDataLockObject);
      if (RequestedData.Contains(args.Id))
      {
        try {
        System.Threading.Monitor.Enter(RequestedData);
        _dataBuffer[args.Id] = args.Value;
        } finally {
        System.Threading.Monitor.Exit(RequestedData);
        }
      }
      //System.Threading.Monitor.Exit(RequestedDataLockObject);
    }

    private void OnDataSetReceived(int id, byte[] payload)
    {
      OnDataSetReceived(new DataSet<byte[]>(id, payload));
    }

    protected override void HandleHandshake()
    {
      PacketSender.SendPacket(PacketSender.Pack(0, 2, 0));
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
