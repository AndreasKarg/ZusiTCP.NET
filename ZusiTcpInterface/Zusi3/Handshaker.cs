using System.ComponentModel;
using System.Linq;
using ZusiTcpInterface.Common;

namespace ZusiTcpInterface.Zusi3
{
  internal class Handshaker
  {
    private readonly IWritableStream _txStream;
    private readonly IReadableStream _rxStream;

    private readonly ClientType _clientType;
    private readonly string _clientName;
    private readonly string _clientVersion;

    public Handshaker(IReadableStream rxStream, IWritableStream txStream, ClientType clientType, string clientName, string clientVersion)
    {
      _txStream = txStream;
      _clientType = clientType;
      _clientName = clientName;
      _clientVersion = clientVersion;
      _rxStream = rxStream;
    }

    public void ShakeHands()
    {
      var hello = new HelloPacket(_clientType, _clientName, _clientVersion);

      _txStream.Write(hello.Serialise());

      var handshakeConverter = new BranchingNodeConverter();
      handshakeConverter[0x02] = new AckHelloConverter();
      
      var rootNodeConverter = new TopLevelNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      var message = Node.Deserialise(_rxStream);

      var ackHello = (AckHelloPacket)rootNodeConverter.Convert(message).Single();

      if(!ackHello.ConnectionAccepted)
        throw new ConnectionRefusedException("Connection refused by Zusi.");
    }
  }
}