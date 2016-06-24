using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class PolledZusiDataReceiver : CallbackBasedZusiDataReceiverBase
  {
    private readonly IBlockingCollection<DataChunkBase> _blockingCollection;

    public PolledZusiDataReceiver(DescriptorCollection descriptors, IBlockingCollection<DataChunkBase> blockingCollection) : base(descriptors)
    {
      _blockingCollection = blockingCollection;
    }

    public PolledZusiDataReceiver(ConnectionContainer connectionContainer) : this(connectionContainer.Descriptors, connectionContainer.ReceivedDataChunks)
    {
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