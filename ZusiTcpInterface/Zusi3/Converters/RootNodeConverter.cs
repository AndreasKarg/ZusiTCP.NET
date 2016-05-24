using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class RootNodeConverter
  {
    private readonly Dictionary<short, INodeConverter> _subNodeConverters = new Dictionary<short, INodeConverter>();

    public Dictionary<short, INodeConverter> SubNodeConverters
    {
      get { return _subNodeConverters; }
    }

    /// <summary>
    /// Equivalent to indexing SubNodeConverters
    /// </summary>
    /// <param name="i">Node Id</param>
    /// <returns>The INodeConverter stored for this id</returns>
    public INodeConverter this[short i]
    {
      get { return _subNodeConverters[i]; }
      set { _subNodeConverters[i] = value; }
    }

    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      return (_subNodeConverters.ContainsKey(node.Id))
        ? _subNodeConverters[node.Id].Convert(node)
        : Enumerable.Empty<IProtocolChunk>();
    }
  }
}