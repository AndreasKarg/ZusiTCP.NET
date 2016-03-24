namespace ZusiTcpInterface.Zusi3
{
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