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
      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, ProtocolVersion)},
        { 0x02, new Attribute(0x02, (short) _clientType)},
        { 0x03, new Attribute(0x03, _clientName)}, 
        { 0x04, new Attribute(0x04, _clientVersion)}
      };

      var helloNode = new Node(HelloNodeId, attributes);

      var topLevelNode = new Node((short)NodeCategory, helloNode);

      return topLevelNode.Serialise();
    }
  }
}
