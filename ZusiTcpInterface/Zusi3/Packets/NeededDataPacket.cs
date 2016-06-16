using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Packets
{
  internal class NeededDataPacket
  {
    private const short ClientApplicationNodeId = 0x02;
    private const short NeededDataNodeId = 0x03;
    private const short CabInfoNodeId = 0x0A;
    private const short NeededDataAttributeId = 0x01;

    private readonly List<short> _neededIds;

    public NeededDataPacket(IEnumerable<short> neededIds)
    {
      _neededIds = neededIds.ToList();
    }

    public List<short> NeededIds
    {
      get { return _neededIds; }
    }

    public void Serialise(BinaryWriter binaryWriter)
    {
      var attributes = new Dictionary<short, Attribute>();

      foreach (var id in _neededIds)
      {
        attributes.Add(id, new Attribute(NeededDataAttributeId, id));
      }

      var neededCabInfoNode = new Node(CabInfoNodeId, attributes);
      var neededDataNode = new Node(NeededDataNodeId, neededCabInfoNode);
      var topLevelNode = new Node(ClientApplicationNodeId, neededDataNode);

      topLevelNode.Serialise(binaryWriter);
    }
  }
}
