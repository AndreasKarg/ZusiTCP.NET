using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterfaceTests.Doubles;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class AckHelloPacketTests
  {
    readonly MockReadableStream _mockReadableStream = new MockReadableStream();

    [TestMethod]
    public void Deserialises_well_formed_packet()
    {
      // Given
      byte[] packet =
      {
        0x00, 0x00, 0x00, 0x00,
          0x01, 0x00,
          0x00, 0x00, 0x00, 0x00,
            0x02, 0x00,
            0x09, 0x00, 0x00, 0x00,
              0x01, 0x00,
              0x33, 0x2E, 0x30, 0x2E, 0x31, 0x2E, 0x30,
            0x03, 0x00, 0x00, 0x00,
              0x02, 0x00,
              0x30,
            0x03, 0x00, 0x00, 0x00,
              0x03, 0x00,
              0x00,
          0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF
      };

      _mockReadableStream.SetDataToRead(packet);

      // When
      var deserialised = AckHelloPacket.Deserialise(_mockReadableStream.Stream);

      // Then
      Assert.AreEqual("3.0.1.0", deserialised.ZusiVersion);
      Assert.AreEqual("0", deserialised.ConnectionInfo);
      Assert.AreEqual(true, deserialised.ConnectionAccepted);
    }
  }
}
