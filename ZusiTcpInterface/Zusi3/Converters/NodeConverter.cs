﻿using System;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class NodeConverter : INodeConverter
  {
    private Dictionary<short, Func<short, byte[], IProtocolChunk>> _conversionFunctions = new Dictionary<short, Func<short, byte[], IProtocolChunk>>();
    private Dictionary<short, INodeConverter> _subNodeConverters = new Dictionary<short, INodeConverter>();

    public Dictionary<short, INodeConverter> SubNodeConverters
    {
      get { return _subNodeConverters; }
      set { _subNodeConverters = value; }
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
        Func<short, byte[], IProtocolChunk> attributeConverter;

        if (!_conversionFunctions.TryGetValue(attribute.Key, out attributeConverter))
          continue;

        chunks.Add(attributeConverter(attribute.Key, attribute.Value.Payload));
      }

      foreach (var subNode in node.SubNodes)
      {
        INodeConverter nodeConverter;

        if(!SubNodeConverters.TryGetValue(subNode.Key, out nodeConverter))
          continue;

        chunks.AddRange(nodeConverter.Convert(subNode.Value));
      }

      return chunks;
    }
  }
}