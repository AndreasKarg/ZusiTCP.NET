using System;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  internal class CabDataConverter : INodeConverter
  {
    private Dictionary<short, Func<short, byte[], IProtocolChunk>> _conversionFunctions = new Dictionary<short, Func<short, byte[], IProtocolChunk>>();
    private readonly BranchingNodeConverter _subNodeConverter = new BranchingNodeConverter();

    public Dictionary<short, INodeConverter> SubNodeConverters
    {
      get { return _subNodeConverter.SubNodeConverters; }
      set { _subNodeConverter.SubNodeConverters = value; }
    }

    public Dictionary<short, Func<short, byte[], IProtocolChunk>> ConversionFunctions
    {
      get { return _conversionFunctions; }
      set { _conversionFunctions = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      var chunks = new List<IProtocolChunk>();

      foreach (var attribute in node.Attributes)
      {
        Func<short, byte[], IProtocolChunk> conversionFunction;

        if (!_conversionFunctions.TryGetValue(attribute.Key, out conversionFunction))
          continue;

        chunks.Add(conversionFunction(attribute.Key, attribute.Value.Payload));
      }

      chunks.AddRange(_subNodeConverter.Convert(node));

      return chunks;
    }
  }
}