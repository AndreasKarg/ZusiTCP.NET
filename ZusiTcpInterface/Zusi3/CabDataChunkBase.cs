namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class CabDataChunkBase : IProtocolChunk
  {
    private readonly Address _id;

    protected CabDataChunkBase(Address id)
    {
      _id = id;
    }

    public Address Id
    {
      get { return _id; }
    }
  }
}