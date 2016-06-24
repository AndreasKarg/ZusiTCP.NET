using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;
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

      var neededData = new List<CabInfoAddress>
      {
        new CabInfoAddress(0x01),
        new CabInfoAddress(0x1B)
      };

      // When
      var serialised = new MemoryStream();
      var binaryWriter = new BinaryWriter(serialised);

      NeededDataPacket.Serialise(binaryWriter, neededData);

      CollectionAssert.AreEqual(expected, serialised.ToArray());
    }
  }
}
