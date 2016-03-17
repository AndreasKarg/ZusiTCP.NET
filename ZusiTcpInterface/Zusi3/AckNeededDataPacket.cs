namespace ZusiTcpInterface.Zusi3
{
  internal class AckNeededDataPacket : IProtocolChunk
  {
    private readonly bool _requestAccepted;

    public AckNeededDataPacket(bool requestAccepted)
    {
      _requestAccepted = requestAccepted;
    }

    public bool RequestAccepted
    {
      get { return _requestAccepted; }
    }
  }
}