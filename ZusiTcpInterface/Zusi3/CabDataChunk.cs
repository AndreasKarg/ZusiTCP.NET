namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class CabDataChunk<T> : CabDataChunkBase
  {
    private readonly T _payload;

    public CabDataChunk(short id, T payload)
      :base(id)
    {
      _payload = payload;
    }

    public T Payload
    {
      get { return _payload; }
    }
  }
}