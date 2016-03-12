using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterfaceTests.Doubles;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class HandshakerTests
  {
    private readonly MockWritableStream _mockWritableStream = new MockWritableStream();

    [TestMethod]
    public void Sends_correct_hello_packet()
    {
      // Given
      byte[] expectedTxData
        = {0x00, 0x00, 0x00, 0x00,
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
              0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF};

      var handshaker = new Handshaker(_mockWritableStream.Stream, ClientType.ControlDesk, "Fahrpult", "2.0");

      // When
      handshaker.ShakeHands();

      // Then
      CollectionAssert.AreEqual(expectedTxData, _mockWritableStream.WrittenData);
    }
  }
}
