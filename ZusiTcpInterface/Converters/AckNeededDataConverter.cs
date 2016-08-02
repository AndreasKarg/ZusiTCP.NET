using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.DOM;
using ZusiTcpInterface.Packets;

namespace ZusiTcpInterface.Converters
{
  internal class AckNeededDataConverter : INodeConverter
  {
    public IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node node)
    {
      var attribute = node.Attributes.Single().Value;
      var requestAccepted = attribute.Payload[0] == 0;

      return new List<IProtocolChunk>() {new AckNeededDataPacket(requestAccepted)};
    }
  }
}