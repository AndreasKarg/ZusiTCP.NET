using System;

namespace ZusiTcpInterface.Zusi3
{
  public class PolledZusiDataReceiver
  {
    private readonly IBlockingCollection<CabDataChunkBase> _blockingCollection;

    public PolledZusiDataReceiver(IBlockingCollection<CabDataChunkBase> blockingCollection)
    {
      _blockingCollection = blockingCollection;
    }

    public event EventHandler<DataReceivedEventArgs<Single>> FloatReceived;

    public event EventHandler<DataReceivedEventArgs<bool>> BoolReceived;

    public void Service()
    {
      while (_blockingCollection.Count != 0)
      {
        RaiseEventFor(_blockingCollection.Take());
      }
    }

    private void RaiseEventFor(CabDataChunkBase chunk)
    {
      if (RaiseEventIfChunkIs<float>(chunk, OnFloatReceived))
        return;

      if (RaiseEventIfChunkIs<bool>(chunk, OnBoolReceived))
        return;

      throw new NotSupportedException("The data type received is not supported.");
    }

    private bool RaiseEventIfChunkIs<T>(CabDataChunkBase chunk, Action<T> handler)
    {
      var dataChunk = chunk as CabDataChunk<T>;
      if (dataChunk == null) return false;

      handler(dataChunk.Payload);
      return true;
    }

    protected virtual void OnFloatReceived(float payload)
    {
      var handler = FloatReceived;
      if (handler != null) handler(this, new DataReceivedEventArgs<float>(payload, 123, "Lalala"));
    }

    protected virtual void OnBoolReceived(bool payload)
    {
      var handler = BoolReceived;
      if (handler != null) handler(this, new DataReceivedEventArgs<bool>(payload, 123, "OLOLOL"));
    }
  }
}