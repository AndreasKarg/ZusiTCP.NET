using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi2;
using ZusiTcpInterfaceTests.Helpers;

namespace ZusiTcpInterfaceTests.Zusi2
{
  [TestClass]
  public class AckHelloPacketTests
  {
    private readonly MemoryStream _rxStream = new MemoryStream();
    private readonly BinaryReader _binaryReader;

    public AckHelloPacketTests()
    {
      _binaryReader = new BinaryReader(_rxStream);
    }

    [TestMethod]
    public void Deserialises_correctly_formatted_packet()
    {
      // Given
      byte[] serialisedAckHello = {0x03, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00};
      _rxStream.ReinitialiseWith(serialisedAckHello);
      
      // When
      var ackHello = AckHelloPacket.Deserialise(_binaryReader);

      // Then
      Assert.IsTrue(ackHello.Acknowledged);
    }

    //Todo: Add rainy day scenarios, e.g. malformed packets etc
  }
}