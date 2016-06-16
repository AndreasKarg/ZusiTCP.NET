using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class PolledZusiDataReceiver : EventRaisingZusiDataReceiverBase
  {
    private readonly IBlockingCollection<DataChunkBase> _blockingCollection;

    public PolledZusiDataReceiver(IBlockingCollection<DataChunkBase> blockingCollection, NodeDescriptor rootNode) : base(rootNode)
    {
      _blockingCollection = blockingCollection;
    }

    public PolledZusiDataReceiver(ConnectionContainer connectionContainer) : this(connectionContainer.ReceivedDataChunks, connectionContainer.Descriptors)
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