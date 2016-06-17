using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using ZusiTcpInterface.Zusi3.DOM;
using Attribute = ZusiTcpInterface.Zusi3.DOM.Attribute;

namespace ZusiTcpInterface.Zusi3.Packets
{
  internal struct HelloPacket
  {
    private const NodeCategory NodeCategory = DOM.NodeCategory.Handshake;
    private const short HelloNodeId = 0x01;

    private const short ProtocolVersion = 2;

    private readonly string _clientName;
    private readonly string _clientVersion;

    private readonly ClientType _clientType;

    public HelloPacket(ClientType clientType, string clientName, string clientVersion) : this()
    {
      _clientName = clientName;
      _clientVersion = clientVersion;
      _clientType = clientType;
    }

    [Pure]
    public ClientType ClientType
    {
      get { return _clientType; }
    }

    [Pure]
    public String ClientName
    {
      get { return _clientName; }
    }

    public void Serialise(BinaryWriter binaryWriter)
    {
      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, ProtocolVersion)},
        { 0x02, new Attribute(0x02, (short) _clientType)},
        { 0x03, new Attribute(0x03, _clientName)}, 
        { 0x04, new Attribute(0x04, _clientVersion)}
      };

      var helloNode = new Node(HelloNodeId, attributes);

      var topLevelNode = new Node((short)NodeCategory, helloNode);

      topLevelNode.Serialise(binaryWriter);
    }
  }
}
