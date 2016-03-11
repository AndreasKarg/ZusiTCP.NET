using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface;

namespace ZusiTcpInterfaceTests
{
  [TestClass]
  public class AckHelloPacketTests
  {
    readonly MockReadableStream _mockReadableStream = new MockReadableStream();

    [TestMethod]
    public void Deserialises_correctly_formatted_packet()
    {
      // Given
      byte[] serialisedAckHello = {0x03, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00};
      _mockReadableStream.SetDataToRead(serialisedAckHello);
      
      // When
      var ackHello = AckHelloPacket.Deserialise(_mockReadableStream.Stream);

      // Then
      Assert.IsTrue(ackHello.Acknowledged);
    }

    //Todo: Add rainy day scenarios, e.g. malformed packets etc
  }
}