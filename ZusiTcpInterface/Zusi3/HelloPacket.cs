using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ZusiTcpInterface.Zusi3
{
  public struct HelloPacket
  {
    private const NodeCategory NodeCategory = Zusi3.NodeCategory.Handshake;
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

    [Pure]
    public IEnumerable<byte> Serialise()
    {
      var attributes = new List<Attribute>()
      {
        new Attribute(0x01, ProtocolVersion),
        new Attribute(0x02, (short) _clientType),
        new Attribute(0x03, _clientName), 
        new Attribute(0x04, _clientVersion)
      };

      var helloNode = new Node<Attribute>(HelloNodeId, attributes);

      var topLevelNode = new Node<Node<Attribute>>((short)NodeCategory, helloNode);

      return topLevelNode.Serialise();
    }
  }
}
