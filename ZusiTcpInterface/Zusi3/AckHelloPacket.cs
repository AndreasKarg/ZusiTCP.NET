using System.Linq;
using System.Text;
using ZusiTcpInterface.Common;

namespace ZusiTcpInterface.Zusi3
{
  internal struct AckHelloPacket
  {
    private const short AckHelloNodeId = 0x02;

    private readonly string _zusiVersion;
    private readonly string _connectionInfo;
    private readonly bool _connectionAccepted;

    private AckHelloPacket(string zusiVersion, string connectionInfo, bool connectionAccepted) : this()
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

    public static AckHelloPacket Deserialise(IReadableStream rxStream)
    {
      var topLevelNode = Node.Deserialise(rxStream);

      var ackHelloNode = topLevelNode.SubNodes[AckHelloNodeId];

      var attributes = ackHelloNode.Attributes;

      var zusiVersion = Encoding.ASCII.GetString(attributes[0x01].Payload);
      var connectionInfo = Encoding.ASCII.GetString(attributes[0x02].Payload);
      var connectionAccepted = attributes[0x03].Payload.Single() == 0;

      return new AckHelloPacket(zusiVersion, connectionInfo, connectionAccepted);
    }
  }
}