using System;
using System.Linq;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class EventRaisingZusiDataReceiverBase
  {
    private readonly CabInfoNodeDescriptor _rootNode;

    protected EventRaisingZusiDataReceiverBase(CabInfoNodeDescriptor rootNode)
    {
      _rootNode = rootNode;
    }

    public event EventHandler<DataReceivedEventArgs<float>> FloatReceived;

    public event EventHandler<DataReceivedEventArgs<bool>> BoolReceived;

    public event EventHandler<DataReceivedEventArgs<SifaStatus>> SifaStatusReceived;

    protected void RaiseEventFor(CabDataChunkBase chunk)
    {
      if (RaiseEventIfChunkIs<float>(chunk, FloatReceived))
        return;

      if (RaiseEventIfChunkIs<bool>(chunk, BoolReceived))
        return;

      if (RaiseEventIfChunkIs<SifaStatus>(chunk, SifaStatusReceived))
        return;

      var payloadType = chunk.GetType().GenericTypeArguments.Single();
      throw new NotSupportedException(String.Format("The data type received ({0}) is not supported.", payloadType));
    }

    private bool RaiseEventIfChunkIs<T>(CabDataChunkBase chunk, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      var dataChunk = chunk as CabDataChunk<T>;
      if (dataChunk == null) return false;

      var eventArgs = new DataReceivedEventArgs<T>(dataChunk.Payload, dataChunk.Address, _rootNode.FindDescriptor(dataChunk.Address));
      if (handler != null)
        handler(this, eventArgs);

      return true;
    }
  }
}