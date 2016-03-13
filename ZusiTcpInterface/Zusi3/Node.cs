using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using MiscUtil.Conversion;
using ZusiTcpInterface.Common;

namespace ZusiTcpInterface.Zusi3
{
  internal class Node : IProtocolElement
  {
    private readonly Dictionary<short, Node> _subNodes;
    private readonly Dictionary<short, Attribute> _attributes;
    private readonly short _id;

    private static readonly LittleEndianBitConverter BitConverter = EndianBitConverter.Little;

    private const uint NodeStarter = 0x00000000;
    private const uint NodeTerminator = 0xFFFFFFFF;

    public short Id
    {
      get { return _id; }
    }

    public Dictionary<short, Node> SubNodes
    {
      get { return _subNodes; }
    }

    public Dictionary<short, Attribute> Attributes
    {
      get { return _attributes; }
    }

    public Node(short id, Dictionary<short, Node> subNodes, Dictionary<short, Attribute> attributes)
    {
      _id = id;
      
      _subNodes = subNodes;
      _attributes = attributes;
    }

    public Node(short id, Dictionary<short, Attribute> attributes)
      : this(id, new Dictionary<short, Node>(), attributes)
    {
    }

    public Node(short id, Node subNode)
      : this(id, new Dictionary<short, Node>(), new Dictionary<short, Attribute>())
    {
      _subNodes.Add(subNode.Id, subNode);
    }

    public IEnumerable<byte> Serialise()
    {
      var serialisedSubNodes = _subNodes.SelectMany(child => child.Value.Serialise());
      var serialisedAttributes = _attributes.SelectMany(child => child.Value.Serialise());

      return BitConverter.GetBytes(NodeStarter)
            .Concat(BitConverter.GetBytes(Id))
            .Concat(serialisedSubNodes)
            .Concat(serialisedAttributes)
            .Concat(BitConverter.GetBytes(NodeTerminator));
    }

    [Pure]
    public static Node Deserialise(IReadableStream rxStream, bool nodeStarterHasAlreadyBeenConsumed = false)
    {
      var subNodes = new Dictionary<short, Node>();
      var attributes = new Dictionary<short, Attribute>();

      if (!nodeStarterHasAlreadyBeenConsumed)
      {
        var nodeStarter = rxStream.Read(4); // Todo: Introduce rainy day scenario that tests this when malformed
      }

      short id = BitConverter.ToInt16(rxStream.Read(2), 0);

      var nextTag = BitConverter.ToUInt32(rxStream.Read(4), 0);

      while (nextTag != NodeTerminator)
      {
        if (nextTag == NodeStarter)
        {
          var newNode = Node.Deserialise(rxStream, true);
          subNodes.Add(newNode.Id, newNode);
        }
        else
        {
          var newAttribute = Attribute.Deserialise(rxStream, (int)nextTag);
          attributes.Add(newAttribute.Id, newAttribute);
        }

        nextTag = BitConverter.ToUInt32(rxStream.Read(4), 0);
      }

      return new Node(id, subNodes, attributes);
    }
  }
}
