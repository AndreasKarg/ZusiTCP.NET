#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  internal class ZusiTcpServerSlaveConnection : ZusiTcpMasterAbstract
  {
    private IBinaryIO _InitializerClient;

    public ZusiTcpServerSlaveConnection(SynchronizationContext hostContext,
                                    IBinaryIO client,
                                    String clientId,
                                    ClientPriority priority)
      : base(clientId, hostContext, priority)
    {
      _InitializerClient = client;
      //InitializeClient(client); //WARNING: Slave will no longer be Initialized automaticly. Server has to do it manually.
    }

    public void InitializeClient()
    {
      InitializeClient(_InitializerClient);
    }

    protected override void HandleHandshake()
    {
      SendPacket(Pack(0, 2, 0));
      ExpectRequestData();
    }

    public void DataUpdate(byte[] array, int id) //ToDo: Improve method name.
    {
      if (RequestedData.Contains(id))
        base.SendByteCommand(array, id);
    }

  }

  internal class DataRequestedEventArgs : EventArgs
  {
    private readonly ICollection<int> _requestedIds;

    public DataRequestedEventArgs(ICollection<int> requestedIds)
    {
      _requestedIds = requestedIds;
    }

    public ICollection<int> RequestedIds
    {
      get { return _requestedIds; }
    }
  }
}
