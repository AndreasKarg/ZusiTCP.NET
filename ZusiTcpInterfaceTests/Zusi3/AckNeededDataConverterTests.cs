using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class AckNeededDataConverterTests
  {
    private readonly AckNeededDataConverter _ackNeededDataConverter = new AckNeededDataConverter();

    [TestMethod]
    public void Deserialises_well_formed_positive_response()
    {
      // Given
      byte[] packet =
      {
        0x00, 0x00, 0x00, 0x00,
          0x04, 0x00,
          0x03, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x00,
        0xFF, 0xFF, 0xFF, 0xFF
      };
      var rxStream = new MemoryStream(packet);
      var binaryReader = new BinaryReader(rxStream, Encoding.ASCII);

      // When
      var deserialised = Node.Deserialise(binaryReader);
      var converted = (AckNeededDataPacket)_ackNeededDataConverter.Convert(deserialised).Single();

      // Then
      Assert.IsTrue(converted.RequestAccepted);
    }
  }
}