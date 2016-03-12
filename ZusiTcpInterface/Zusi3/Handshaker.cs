using ZusiTcpInterface.Common;

namespace ZusiTcpInterface.Zusi3
{
  internal class Handshaker
  {
    private readonly IWritableStream _txStream;
    private readonly ClientType _clientType;
    private readonly string _clientName;
    private readonly string _clientVersion;

    public Handshaker(IWritableStream txStream, ClientType clientType, string clientName, string clientVersion)
    {
      _txStream = txStream;
      _clientType = clientType;
      _clientName = clientName;
      _clientVersion = clientVersion;
    }

    public void ShakeHands()
    {
      var hello = new HelloPacket(_clientType, _clientName, _clientVersion);

      _txStream.Write(hello.Serialise());
    }
  }
}