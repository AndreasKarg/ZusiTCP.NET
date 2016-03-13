using System.IO;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  internal class Handshaker
  {
    private readonly BinaryWriter _binaryWriter;
    private readonly BinaryReader _binaryReader;

    private readonly ClientType _clientType;
    private readonly string _clientName;
    private readonly string _clientVersion;

    public Handshaker(BinaryReader binaryReader, BinaryWriter binaryWriter, ClientType clientType, string clientName, string clientVersion)
    {
      _binaryWriter = binaryWriter;
      _clientType = clientType;
      _clientName = clientName;
      _clientVersion = clientVersion;
      _binaryReader = binaryReader;
    }

    public void ShakeHands()
    {
      var hello = new HelloPacket(_clientType, _clientName, _clientVersion);

      hello.Serialise(_binaryWriter);

      var handshakeConverter = new BranchingNodeConverter();
      handshakeConverter[0x02] = new AckHelloConverter();
      
      var rootNodeConverter = new TopLevelNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      var message = Node.Deserialise(_binaryReader);

      var ackHello = (AckHelloPacket)rootNodeConverter.Convert(message).Single();

      if(!ackHello.ConnectionAccepted)
        throw new ConnectionRefusedException("Connection refused by Zusi.");
    }
  }
}