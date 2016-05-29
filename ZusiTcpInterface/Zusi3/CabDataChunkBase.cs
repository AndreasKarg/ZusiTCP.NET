namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class CabDataChunkBase : IProtocolChunk
  {
    private readonly Address _address;

    protected CabDataChunkBase(Address address)
    {
      _address = address;
    }

    public Address Address
    {
      get { return _address; }
    }
  }
}