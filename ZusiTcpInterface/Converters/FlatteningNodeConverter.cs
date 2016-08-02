using System;
using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.DOM;

namespace ZusiTcpInterface.Converters
{
  internal class FlatteningNodeConverter : INodeConverter
  {
    private Dictionary<Address, Func<Address, byte[], IProtocolChunk>> _conversionFunctions = new Dictionary<Address, Func<Address, byte[], IProtocolChunk>>();

    public Dictionary<Address, Func<Address, byte[], IProtocolChunk>> ConversionFunctions
    {
      get { return _conversionFunctions; }
      set { _conversionFunctions = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node node)
    {
      var chunks = new List<IProtocolChunk>();
      var nodeAddress = baseAddress.Concat(node.Id);

      foreach (var attribute in node.Attributes)
      {
        var attributeAddress = nodeAddress.Concat(attribute.Key);
        Func<Address, byte[], IProtocolChunk> attributeConverter;

        if (!_conversionFunctions.TryGetValue(attributeAddress, out attributeConverter))
          continue;

        chunks.Add(attributeConverter(nodeAddress.Concat(attribute.Key), attribute.Value.Payload));
      }

      var childNodeChunks = node.ChildNodes.SelectMany(childNode => Convert(nodeAddress, childNode));

      return chunks.Concat(childNodeChunks);
    }
  }
}