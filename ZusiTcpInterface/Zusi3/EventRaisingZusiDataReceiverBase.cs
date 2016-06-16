using System;
using System.Linq;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class EventRaisingZusiDataReceiverBase
  {
    private readonly NodeDescriptor _rootNode;

    protected EventRaisingZusiDataReceiverBase(NodeDescriptor rootNode)
    {
      _rootNode = rootNode;
    }

    public event EventHandler<DataReceivedEventArgs<float>> FloatReceived;

    public event EventHandler<DataReceivedEventArgs<bool>> BoolReceived;

    protected void RaiseEventFor(DataChunkBase chunk)
    {
      if (RaiseEventIfChunkIs<float>(chunk, FloatReceived))
        return;

      if (RaiseEventIfChunkIs<bool>(chunk, BoolReceived))
        return;

      var payloadType = chunk.GetType().GenericTypeArguments.Single();
      throw new NotSupportedException(String.Format("The data type received ({0}) is not supported.", payloadType));
    }

    private bool RaiseEventIfChunkIs<T>(DataChunkBase chunk, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      var dataChunk = chunk as DataChunk<T>;
      if (dataChunk == null) return false;

      var eventArgs = new DataReceivedEventArgs<T>(dataChunk.Payload, dataChunk.Address, _rootNode.FindDescriptor(dataChunk.Address));
      if (handler != null)
        handler(this, eventArgs);

      return true;
    }
  }
}