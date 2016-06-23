namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class DataChunk<T> : DataChunkBase
  {
    private readonly T _payload;

    public DataChunk(Address address, T payload)
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