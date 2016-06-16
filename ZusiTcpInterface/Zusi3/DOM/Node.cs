using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace ZusiTcpInterface.Zusi3.DOM
{
  internal class Node
  {
    private readonly List<Node> _subNodes;
    private readonly Dictionary<short, Attribute> _attributes;
    private readonly short _id;

    private const uint NodeStarter = 0x00000000;
    private const uint NodeTerminator = 0xFFFFFFFF;

    public short Id
    {
      get { return _id; }
    }

    public List<Node> SubNodes
    {
      get { return _subNodes; }
    }

    public Dictionary<short, Attribute> Attributes
    {
      get { return _attributes; }
    }

    public Node(short id, List<Node> subNodes, Dictionary<short, Attribute> attributes)
    {
      _id = id;

      _subNodes = subNodes;
      _attributes = attributes;
    }

    public Node(short id, Dictionary<short, Attribute> attributes)
      : this(id, new List<Node>(), attributes)
    {
    }

    public Node(short id, Node subNode)
      : this(id, new List<Node>(), new Dictionary<short, Attribute>())
    {
      _subNodes.Add(subNode);
    }

    public void Serialise(BinaryWriter binaryWriter)
    {
      binaryWriter.Write(NodeStarter);
      binaryWriter.Write(Id);
      foreach (var subNode in _subNodes)
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
    public static Node Deserialise(BinaryReader binaryReader, bool nodeStartTagHasAlreadyBeenConsumed = false)
    {
      var subNodes = new List<Node>();
      var attributes = new Dictionary<short, Attribute>();

      if (!nodeStartTagHasAlreadyBeenConsumed)
      {
        var nodeStartTag = binaryReader.ReadInt32(); // Todo: Introduce rainy day scenario that tests this when malformed
      }

      short id = binaryReader.ReadInt16();

      var nextTag = binaryReader.ReadUInt32();

      while (nextTag != NodeTerminator)
      {
        if (nextTag == NodeStarter)
        {
          var newNode = Node.Deserialise(binaryReader, true);
          subNodes.Add(newNode);
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

    public IEnumerable<String> DumpToStrings()
    {
      yield return String.Format("Node 0x{0:x2}:", Id);

      foreach (var attribute in Attributes.Values)
      {
        var attributeDump = attribute.DumpToStrings();
        foreach (var line in attributeDump)
        {
          yield return "  " + line;
        }
      }

      foreach (var subNode in SubNodes)
      {
        var nodeDump = subNode.DumpToStrings();
        foreach (var line in nodeDump)
        {
          yield return "  " + line;
        }
      }
    }
  }
}