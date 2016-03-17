using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  internal class AckNeededDataConverter : INodeConverter
  {
    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      var attribute = node.Attributes.Single().Value;
      var requestAccepted = attribute.Payload[0] == 0;

      return new List<IProtocolChunk>() {new AckNeededDataPacket(requestAccepted)};
    }
  }
}