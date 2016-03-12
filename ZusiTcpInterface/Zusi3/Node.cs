using System.Collections.Generic;
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

    private static readonly byte[] NodeStarter = { 0x00, 0x00, 0x00, 0x00 };
    private static readonly byte[] NodeTerminator = { 0xFF, 0xFF, 0xFF, 0xFF };
    private static readonly LittleEndianBitConverter BitConverter = EndianBitConverter.Little;

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

      return NodeStarter.Concat(BitConverter.GetBytes(Id))
            .Concat(serialisedSubNodes)
            .Concat(serialisedAttributes)
            .Concat(NodeTerminator);
    }

    public static Node Deserialise(IReadableStream rxStream)
    {
      var subNodes = new Dictionary<short, Node>();
      var attributes = new Dictionary<short, Attribute>();

      var length = rxStream.Read(4); // Todo: Introduce rainy day scenario that tests this when malformed

      short id = BitConverter.ToInt16(rxStream.Read(2), 0);

      var nextTag = rxStream.Peek(4);

      while (!nextTag.SequenceEqual(NodeTerminator))
      {
        if (nextTag.SequenceEqual(NodeStarter))
        {
          var newNode = Node.Deserialise(rxStream);
          subNodes.Add(newNode.Id, newNode);
        }
        else
        {
          var newAttribute = Attribute.Deserialise(rxStream);
          attributes.Add(newAttribute.Id, newAttribute);
        }

        nextTag = rxStream.Peek(4);
      }

      rxStream.Read(4); // Skip end tag

      return new Node(id, subNodes, attributes);
    }
  }
}
