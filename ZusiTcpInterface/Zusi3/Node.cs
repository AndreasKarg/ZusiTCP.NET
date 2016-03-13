using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace ZusiTcpInterface.Zusi3
{
  internal class Node : IProtocolElement
  {
    private readonly Dictionary<short, Node> _subNodes;
    private readonly Dictionary<short, Attribute> _attributes;
    private readonly short _id;

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

    public void Serialise(BinaryWriter binaryWriter)
    {
      binaryWriter.Write(NodeStarter);
      binaryWriter.Write(Id);
      foreach (var subNode in _subNodes.Values)
      {
        subNode.Serialise(binaryWriter);
      }

      foreach (var attribute in _attributes.Values)
      {
        attribute.Serialise(binaryWriter);
      }
      binaryWriter.Write(NodeTerminator);
    }

    [Pure]
    public static Node Deserialise(BinaryReader binaryReader, bool nodeStarterHasAlreadyBeenConsumed = false)
    {
      var subNodes = new Dictionary<short, Node>();
      var attributes = new Dictionary<short, Attribute>();

      if (!nodeStarterHasAlreadyBeenConsumed)
      {
        var nodeStarter = binaryReader.ReadInt32(); // Todo: Introduce rainy day scenario that tests this when malformed
      }

      short id = binaryReader.ReadInt16();

      var nextTag = binaryReader.ReadUInt32();

      while (nextTag != NodeTerminator)
      {
        if (nextTag == NodeStarter)
        {
          var newNode = Node.Deserialise(binaryReader, true);
          subNodes.Add(newNode.Id, newNode);
        }
        else
        {
          var newAttribute = Attribute.Deserialise(binaryReader, (int)nextTag);
          attributes.Add(newAttribute.Id, newAttribute);
        }

        nextTag = binaryReader.ReadUInt32();
      }

      return new Node(id, subNodes, attributes);
    }
  }
}
