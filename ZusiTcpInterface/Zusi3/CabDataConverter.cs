using System;
using System.Collections.Generic;

namespace ZusiTcpInterface.Zusi3
{
  internal class CabDataConverter : INodeConverter
  {
    private readonly Dictionary<short, Func<short, byte[], IProtocolChunk>> _conversionFunctions;

    public CabDataConverter()
      : this(new Dictionary<short, Func<short, byte[], IProtocolChunk>>())
    {
    }

    public CabDataConverter(Dictionary<short, Func<short, byte[], IProtocolChunk>> conversionFunctions)
    {
      _conversionFunctions = conversionFunctions;
    }

    public Dictionary<short, Func<short, byte[], IProtocolChunk>> ConversionFunctions
    {
      get { return _conversionFunctions; }
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

      return chunks;
    }
  }
}