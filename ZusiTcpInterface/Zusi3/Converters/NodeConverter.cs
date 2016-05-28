using System;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class NodeConverter : INodeConverter
  {
    private Dictionary<short, Func<Address, byte[], IProtocolChunk>> _conversionFunctions = new Dictionary<short, Func<Address, byte[], IProtocolChunk>>();
    private Dictionary<short, INodeConverter> _subNodeConverters = new Dictionary<short, INodeConverter>();
    private readonly Address _address;

    public NodeConverter()
      : this(new Address())
    {
    }

    public NodeConverter(short address)
      : this(new Address(address))
    {
    }

    public NodeConverter(Address address)
    {
      _address = address;
    }

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

    public IEnumerable<IProtocolChunk> Convert(Address accumulatedAddress, Node node)
    {
      var chunks = new List<IProtocolChunk>();
      var fullAddress = new Address(accumulatedAddress, _address);

      foreach (var attribute in node.Attributes)
      {
        Func<Address, byte[], IProtocolChunk> attributeConverter;

        if (!_conversionFunctions.TryGetValue(attribute.Key, out attributeConverter))
          continue;

        chunks.Add(attributeConverter(new Address(fullAddress, attribute.Key), attribute.Value.Payload));
      }

      foreach (var subNode in node.SubNodes)
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