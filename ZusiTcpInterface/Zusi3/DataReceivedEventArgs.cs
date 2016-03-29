using System;

namespace ZusiTcpInterface.Zusi3
{
  public class DataReceivedEventArgs<T> : EventArgs
  {
    private readonly T _payload;
    private readonly short _id;
    private readonly string _name;

    public DataReceivedEventArgs(T payload, short id, string name)
    {
      _payload = payload;
      _name = name;
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

    public string Name
    {
      get { return _name; }
    }
  }
}