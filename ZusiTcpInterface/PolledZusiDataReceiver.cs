﻿using ZusiTcpInterface.TypeDescriptors;

namespace ZusiTcpInterface
{
  public class PolledZusiDataReceiver : CallbackBasedZusiDataReceiverBase
  {
    private readonly IBlockingCollection<DataChunkBase> _blockingCollection;

    public PolledZusiDataReceiver(DescriptorCollection descriptors, IBlockingCollection<DataChunkBase> blockingCollection) : base(descriptors)
    {
      _blockingCollection = blockingCollection;
    }

    public void Service()
    {
      while (_blockingCollection.Count != 0)
      {
        RaiseEventFor(_blockingCollection.Take());
      }
    }
  }
}