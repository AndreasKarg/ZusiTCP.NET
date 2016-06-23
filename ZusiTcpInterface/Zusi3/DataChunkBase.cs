namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class DataChunkBase : IProtocolChunk
  {
    private readonly Address _address;

    protected DataChunkBase(Address address)
    {
      _address = address;
    }

    public Address Address
    {
      get { return _address; }
    }
  }
}