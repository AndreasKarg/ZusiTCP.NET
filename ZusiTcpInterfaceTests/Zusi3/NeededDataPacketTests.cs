using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using ZusiTcpInterface.Zusi3.Packets;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class NeededDataPacketTests
  {
    [TestMethod]
    public void Serialised_NeededData_matches_specification()
    {
      // Given
      byte[] expected = {0x00, 0x00, 0x00, 0x00,
                         0x02, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x03, 0x00,
                              0x00, 0x00, 0x00, 0x00,
                              0x0A, 0x00,
                                0x04, 0x00, 0x00, 0x00,
                                  0x01, 0x00,
                                  0x01, 0x00,
                                0x04, 0x00, 0x00, 0x00,
                                  0x01, 0x00,
                                  0x1B, 0x00,
                              0xFF, 0xFF, 0xFF, 0xFF,
                            0xFF, 0xFF, 0xFF, 0xFF,
                          0xFF, 0xFF, 0xFF, 0xFF};

      var neededData = new List<short> {0x01};

      var neededDataPacket = new NeededDataPacket(neededData);
      neededDataPacket.NeededIds.Add(0x1B);

      // When
      var serialised = new MemoryStream();
      var binaryWriter = new BinaryWriter(serialised);

      neededDataPacket.Serialise(binaryWriter);

      CollectionAssert.AreEqual(expected, serialised.ToArray());
    }
  }
}