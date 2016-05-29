namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class CabDataChunk<T> : CabDataChunkBase
  {
    private readonly T _payload;

    public CabDataChunk(Address address, T payload)
      :base(address)
    {
      _payload = payload;
    }

    public T Payload
    {
      get { return _payload; }
    }
  }
}