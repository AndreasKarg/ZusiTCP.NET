using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZusiTcpInterface.Zusi3.DOM;
using Attribute = ZusiTcpInterface.Zusi3.DOM.Attribute;

namespace ZusiTcpInterface.Zusi3.Packets
{
  internal static class NeededDataPacket
  {
    private const short ClientApplicationNodeId = 0x02;
    private const short NeededDataNodeId = 0x03;
    private const short CabInfoNodeId = 0x0A;
    private const short NeededDataAttributeId = 0x01;

    public static void Serialise(BinaryWriter binaryWriter, IEnumerable<CabInfoAddress> neededAdresses)
    {
      var distictIds = neededAdresses.Select(GetIdFromAddress).Distinct();

      var attributes = distictIds.ToDictionary(id => id, id => new Attribute(NeededDataAttributeId, id));

      var neededCabInfoNode = new Node(CabInfoNodeId, attributes);
      var neededDataNode = new Node(NeededDataNodeId, neededCabInfoNode);
      var topLevelNode = new Node(ClientApplicationNodeId, neededDataNode);

      topLevelNode.Serialise(binaryWriter);
    }

    private static short GetIdFromAddress(CabInfoAddress address)
    {
      return address.CabInfoSpecificIds.First();
    }
  }
}
