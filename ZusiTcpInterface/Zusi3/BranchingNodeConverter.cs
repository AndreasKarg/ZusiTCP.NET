using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  internal class BranchingNodeConverter : INodeConverter
  {
    private readonly Dictionary<short, INodeConverter> _subNodeConverters = new Dictionary<short, INodeConverter>();

    public Dictionary<short, INodeConverter> SubNodeConverters
    {
      get { return _subNodeConverters; }
    }

    public INodeConverter this[short i]
    {
      get { return _subNodeConverters[i]; }
      set { _subNodeConverters[i] = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      return node.SubNodes
        .Where(subNode => _subNodeConverters.ContainsKey(subNode.Key))
        .Select(subNode => _subNodeConverters[subNode.Key].Convert(subNode.Value))
        .Aggregate((accumulated, newChunk) => accumulated.Concat(newChunk));
    }
  }
}