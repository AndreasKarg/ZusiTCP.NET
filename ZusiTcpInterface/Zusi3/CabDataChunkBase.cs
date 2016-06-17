namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class CabDataChunkBase : IProtocolChunk
  {
    private readonly short _id;

    protected CabDataChunkBase(short id)
    {
      _id = id;
    }

    public short Id
    {
      get { return _id; }
    }
  }
}