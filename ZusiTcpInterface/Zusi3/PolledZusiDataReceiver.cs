namespace ZusiTcpInterface.Zusi3
{
  public class PolledZusiDataReceiver : CallbackBasedZusiDataReceiverBase
  {
    private readonly IBlockingCollection<DataChunkBase> _blockingCollection;

    public PolledZusiDataReceiver(IBlockingCollection<DataChunkBase> blockingCollection)
    {
      _blockingCollection = blockingCollection;
    }

    public PolledZusiDataReceiver(ConnectionContainer connectionContainer) : this(connectionContainer.ReceivedDataChunks)
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
