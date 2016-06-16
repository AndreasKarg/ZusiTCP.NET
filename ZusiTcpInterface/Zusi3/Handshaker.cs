using System.Collections.Generic;
using System.IO;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Packets;

namespace ZusiTcpInterface.Zusi3
{
  internal class Handshaker
  {
    private readonly BinaryWriter _binaryWriter;

    private readonly ClientType _clientType;
    private readonly string _clientName;
    private readonly string _clientVersion;
    private readonly IEnumerable<short> _neededData;
    private readonly IBlockingCollection<IProtocolChunk> _rxQueue;

    public Handshaker(IBlockingCollection<IProtocolChunk> rxQueue, BinaryWriter binaryWriter, ClientType clientType, string clientName, string clientVersion, IEnumerable<short> neededData)
    {
      _binaryWriter = binaryWriter;
      _clientType = clientType;
      _clientName = clientName;
      _clientVersion = clientVersion;
      _neededData = neededData;
      _rxQueue = rxQueue;
    }

    public void ShakeHands()
    {
      HelloPacket.Serialise(_binaryWriter, _clientType, _clientName, _clientVersion);

      var handshakeConverter = new NodeConverter();
      handshakeConverter.SubNodeConverters[0x02] = new AckHelloConverter();

      var rootNodeConverter = new RootNodeConverter();
      rootNodeConverter[0x01] = handshakeConverter;

      var ackHello = (AckHelloPacket)_rxQueue.Take();

      if(!ackHello.ConnectionAccepted)
        throw new ConnectionRefusedException("Connection refused by Zusi.");

      NeededDataPacket.Serialise(_binaryWriter, _neededData);

      var ackNeededData = (AckNeededDataPacket) _rxQueue.Take();

      if(!ackNeededData.RequestAccepted)
        throw new ConnectionRefusedException("Needed data rejected by Zusi.");
    }
  }
}
