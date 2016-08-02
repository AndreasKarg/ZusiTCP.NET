using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Packets;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class HelloPacketTests
  {
    [TestMethod]
    public void Serialised_Hello_matches_specification()
    {
      // Given
      byte[] expected = {0x00, 0x00, 0x00, 0x00,
                         0x01, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x01, 0x00,
                              0x04, 0x00, 0x00, 0x00,
                                0x01, 0x00,
                                0x02, 0x00,
                              0x04, 0x00, 0x00, 0x00,
                                0x02, 0x00,
                                0x02, 0x00,
                              0x0A, 0x00, 0x00, 0x00,
                                0x03, 0x00,
                                0x46, 0x61, 0x68, 0x72, 0x70, 0x75, 0x6C, 0x74,
                              0x05, 0x00, 0x00, 0x00,
                                0x04, 0x00,
                                0x32, 0x2E, 0x30,
                            0xFF, 0xFF, 0xFF, 0xFF,
                          0xFF, 0xFF, 0xFF, 0xFF};

      // When
      var serialised = new MemoryStream();
      var binaryWriter = new BinaryWriter(serialised);

      HelloPacket.Serialise(binaryWriter, ClientType.ControlDesk, "Fahrpult", "2.0");

      // Then
      CollectionAssert.AreEqual(expected, serialised.ToArray());
    }
  }
}