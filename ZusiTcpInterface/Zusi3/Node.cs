using System.Collections.Generic;
using System.Linq;
using MiscUtil.Conversion;

namespace ZusiTcpInterface.Zusi3
{
  internal class Node<T> : IProtocolElement where T: IProtocolElement
  {
    private readonly byte[] _nodeId;
    private readonly List<T> _children;

    public List<T> Children
    {
      get { return _children; }
    }

    public Node(short nodeId, List<T> children = null )
    {
      var bitConverter = EndianBitConverter.Little;

      _nodeId = bitConverter.GetBytes(nodeId);
      
      _children = children ?? new List<T>();
    }

    public Node(short nodeId, T child)
      : this(nodeId)
    {
      Children.Add(child);
    }

    public Node(NodeCategory nodeCategory) 
      : this((short) nodeCategory)
    { }

    public IEnumerable<byte> Serialise()
    {
      var serialisedNodes = _children.SelectMany(child => child.Serialise());

      return NodeHelpers.MagicNodeLengthIdentifier
            .Concat(_nodeId)
            .Concat(serialisedNodes)
            .Concat(NodeHelpers.NodeTerminator);
    }
  }

  internal static class NodeHelpers
  {
    public static readonly byte[] MagicNodeLengthIdentifier = {0x00, 0x00, 0x00, 0x00};
    public static readonly byte[] NodeTerminator = { 0xFF, 0xFF, 0xFF, 0xFF };
  }
}
