﻿using System;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  public class PolledZusiDataReceiver
  {
    private readonly IBlockingCollection<CabDataChunkBase> _blockingCollection;
    private readonly DescriptorCollection _descriptorCollection;

    public PolledZusiDataReceiver(IBlockingCollection<CabDataChunkBase> blockingCollection, DescriptorCollection descriptorCollection)
    {
      _blockingCollection = blockingCollection;
      _descriptorCollection = descriptorCollection;
    }

    public PolledZusiDataReceiver(ConnectionContainer connectionContainer) : this(connectionContainer.ReceivedCabDataChunks, connectionContainer.Descriptors)
    {
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
      if (RaiseEventIfChunkIs<float>(chunk, FloatReceived))
        return;

      if (RaiseEventIfChunkIs<bool>(chunk, BoolReceived))
        return;

      var payloadType = chunk.GetType().GenericTypeArguments.Single();
      throw new NotSupportedException(String.Format("The data type received ({0}) is not supported.", payloadType));
    }

    private bool RaiseEventIfChunkIs<T>(CabDataChunkBase chunk, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      var dataChunk = chunk as CabDataChunk<T>;
      if (dataChunk == null) return false;
      if (handler == null)
        return true;

      handler(this, new DataReceivedEventArgs<T>(dataChunk.Payload, dataChunk.Id, _descriptorCollection[dataChunk.Id]));
      return true;
    }
  }
}