namespace ZusiTcpInterface.Zusi3
{
  internal class AckHelloPacket : IProtocolChunk
  {
    private readonly string _zusiVersion;
    private readonly string _connectionInfo;
    private readonly bool _connectionAccepted;

    public AckHelloPacket(string zusiVersion, string connectionInfo, bool connectionAccepted)
    {
      _zusiVersion = zusiVersion;
      _connectionInfo = connectionInfo;
      _connectionAccepted = connectionAccepted;
    }

    public string ZusiVersion
    {
      get { return _zusiVersion; }
    }

    public string ConnectionInfo
    {
      get { return _connectionInfo; }
    }

    public bool ConnectionAccepted
    {
      get { return _connectionAccepted; }
    }
  }
}