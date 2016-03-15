namespace ZusiTcpInterface.Zusi3
{
  internal class CabDataChunk<T> : IProtocolChunk
  {
    private readonly short _id;
    private readonly T _payload;

    public CabDataChunk(short id, T payload)
    {
      _id = id;
      _payload = payload;
    }

    public T Payload
    {
      get { return _payload; }
    }

    public short Id
    {
      get { return _id; }
    }
  }
}