using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class PolledZusiDataReceiver : EventRaisingZusiDataReceiverBase
  {
    private readonly IBlockingCollection<CabDataChunkBase> _blockingCollection;

    public PolledZusiDataReceiver(IBlockingCollection<CabDataChunkBase> blockingCollection, CabInfoNodeDescriptor rootNode) : base(rootNode)
    {
      _blockingCollection = blockingCollection;
    }

    public PolledZusiDataReceiver(ConnectionContainer connectionContainer) : this(connectionContainer.ReceivedCabDataChunks, connectionContainer.CabDataDescriptors)
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