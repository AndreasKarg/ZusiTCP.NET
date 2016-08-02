using System.Collections.Generic;
using System.IO;
using ZusiTcpInterface.Converters;
using ZusiTcpInterface.DOM;
using ZusiTcpInterface.Packets;

namespace ZusiTcpInterface
{
  internal class Handshaker
  {
    private readonly BinaryWriter _binaryWriter;

    private readonly ClientType _clientType;
    private readonly string _clientName;
    private readonly string _clientVersion;
    private readonly IEnumerable<CabInfoAddress> _neededData;
    private readonly IMessageReceiver _messageReceiver;

    public Handshaker(IMessageReceiver messageReceiver, BinaryWriter binaryWriter, ClientType clientType, string clientName, string clientVersion, IEnumerable<CabInfoAddress> neededData)
    {
      _binaryWriter = binaryWriter;
      _clientType = clientType;
      _clientName = clientName;
      _clientVersion = clientVersion;
      _neededData = neededData;
      _messageReceiver = messageReceiver;
    }

    public void ShakeHands()
    {
      HelloPacket.Serialise(_binaryWriter, _clientType, _clientName, _clientVersion);

      var handshakeConverter = new NodeConverter();
      handshakeConverter.SubNodeConverters[0x02] = new AckHelloConverter();

      var rootNodeConverter = new RootNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      var ackHello = (AckHelloPacket)_messageReceiver.GetNextChunk();

      if(!ackHello.ConnectionAccepted)
        throw new ConnectionRefusedException("Connection refused by Zusi.");

      NeededDataPacket.Serialise(_binaryWriter, _neededData);

      var ackNeededData = (AckNeededDataPacket) _messageReceiver.GetNextChunk();

      if(!ackNeededData.RequestAccepted)
        throw new ConnectionRefusedException("Needed data rejected by Zusi.");
    }
  }
}