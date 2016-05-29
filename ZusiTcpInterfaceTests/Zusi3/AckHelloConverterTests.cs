using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Packets;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class AckHelloConverterTests
  {
    private readonly AckHelloConverter _ackHelloConverter = new AckHelloConverter();

    [TestMethod]
    public void Deserialises_well_formed_packet()
    {
      // Given
      byte[] packet =
      {
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
      };
      var rxStream = new MemoryStream(packet);
      var binaryReader = new BinaryReader(rxStream, Encoding.ASCII);

      // When
      var deserialised = Node.Deserialise(binaryReader);
      var converted = (AckHelloPacket)_ackHelloConverter.Convert(new Address(), deserialised).Single();

      // Then
      Assert.AreEqual("3.0.1.0", converted.ZusiVersion);
      Assert.AreEqual("0", converted.ConnectionInfo);
      Assert.AreEqual(true, converted.ConnectionAccepted);
    }
  }
}