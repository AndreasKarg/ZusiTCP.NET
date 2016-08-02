using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using ZusiTcpInterface.DOM;
using ZusiTcpInterface.Packets;

namespace ZusiTcpInterface.Converters
{
  internal class AckHelloConverter : INodeConverter
  {
    private const short ZusiVersionId = 0x01,
                ConnectionInfoId = 0x02,
                ConnectionAcceptedId = 0x03;

    [Pure]
    public IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node ackHelloNode)
    {
      var attributes = ackHelloNode.Attributes;

      var zusiVersion = Encoding.ASCII.GetString(attributes[ZusiVersionId].Payload);
      var connectionInfo = Encoding.ASCII.GetString(attributes[ConnectionInfoId].Payload);
      var connectionAccepted = attributes[ConnectionAcceptedId].Payload.Single() == 0;

      var ackHelloPacket = new AckHelloPacket(zusiVersion, connectionInfo, connectionAccepted);
      return new[] { ackHelloPacket };
    }
  }
}