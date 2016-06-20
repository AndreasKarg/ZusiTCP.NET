using System;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class NodeConverter : INodeConverter
  {
    private Dictionary<short, Func<Address, byte[], IProtocolChunk>> _conversionFunctions = new Dictionary<short, Func<Address, byte[], IProtocolChunk>>();
    private Dictionary<short, INodeConverter> _subNodeConverters = new Dictionary<short, INodeConverter>();

    public Dictionary<short, INodeConverter> SubNodeConverters
    {
      get { return _subNodeConverters; }
      set { _subNodeConverters = value; }
    }

    public Dictionary<short, Func<Address, byte[], IProtocolChunk>> ConversionFunctions
    {
      get { return _conversionFunctions; }
      set { _conversionFunctions = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node node)
    {
      var chunks = new List<IProtocolChunk>();
      var fullAddress = baseAddress.Concat(node.Id);

      foreach (var attribute in node.Attributes)
      {
        Func<Address, byte[], IProtocolChunk> attributeConverter;

        if (!_conversionFunctions.TryGetValue(attribute.Key, out attributeConverter))
          continue;

        chunks.Add(attributeConverter(fullAddress.Concat(attribute.Key), attribute.Value.Payload));
      }

      foreach (var subNode in node.ChildNodes)
      {
        INodeConverter nodeConverter;

        if(!SubNodeConverters.TryGetValue(subNode.Id, out nodeConverter))
          continue;

        chunks.AddRange(nodeConverter.Convert(fullAddress, subNode));
      }

      return chunks;
    }
  }
}
