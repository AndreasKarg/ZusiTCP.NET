using System;

namespace ZusiTcpInterface.Zusi3
{
  public class DataReceivedEventArgs<T> : EventArgs
  {
    private readonly T _payload;
    private readonly short _id;
    private readonly CabInfoTypeDescriptor _descriptor;

    public DataReceivedEventArgs(T payload, short id, CabInfoTypeDescriptor descriptor)
    {
      _payload = payload;
      _descriptor = descriptor;
      _id = id;
    }

    public T Payload
    {
      get { return _payload; }
    }

    public short Id
    {
      get { return _id; }
    }

    public CabInfoTypeDescriptor Descriptor
    {
      get { return _descriptor; }
    }
  }
}