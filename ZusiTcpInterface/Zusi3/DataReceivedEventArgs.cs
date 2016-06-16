using System;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class DataReceivedEventArgs<T> : EventArgs
  {
    private readonly T _payload;
    private readonly Address _id;
    private readonly AttributeDescriptor _descriptor;

    public DataReceivedEventArgs(T payload, Address id, AttributeDescriptor descriptor)
    {
      _payload = payload;
      _descriptor = descriptor;
      _id = id;
    }

    public T Payload
    {
      get { return _payload; }
    }

    public Address Id
    {
      get { return _id; }
    }

    public AttributeDescriptor Descriptor
    {
      get { return _descriptor; }
    }
  }
}