using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Packets;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class AckNeededDataConverter : INodeConverter
  {
    public IEnumerable<IProtocolChunk> Convert(Address accumulatedAddress, Node node)
    {
      var attribute = node.Attributes.Single().Value;
      var requestAccepted = attribute.Payload[0] == 0;

      return new List<IProtocolChunk>() {new AckNeededDataPacket(requestAccepted)};
    }
  }
}