using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class StringDumpNodeConverterTests
  {
    private readonly StringDumpNodeConverter _stringDumpNodeConverter = new StringDumpNodeConverter();

    [TestMethod]
    public void Dumps_attributes_of_individual_node()
    {
      // Given
      byte[] packet =
      {
        0x00, 0x00, 0x00, 0x00,
          0x64, 0x00,
          0x09, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x33, 0x2E, 0x30, 0x2E, 0x31, 0x2E, 0x30,
          0x03, 0x00, 0x00, 0x00,
            0x02, 0x00,
            0x01,
          0x03, 0x00, 0x00, 0x00,
            0x03, 0x00,
            0x02,
          0x03, 0x00, 0x00, 0x00,
            0x04, 0x00,
            0x01,
          0x03, 0x00, 0x00, 0x00,
            0x05, 0x00,
            0x02,
          0x03, 0x00, 0x00, 0x00,
            0x06, 0x00,
            0x01,
        0xFF, 0xFF, 0xFF, 0xFF,
      };
      var rxStream = new MemoryStream(packet);
      var binaryReader = new BinaryReader(rxStream, Encoding.ASCII);

      var expectedString =
@"Node 0x64:
  Attribute 0x01:
    0x33, 0x2E, 0x30, 0x2E, 0x31, 0x2E, 0x30
  Attribute 0x02:
    0x01
  Attribute 0x03:
    0x02
  Attribute 0x04:
    0x01
  Attribute 0x05:
    0x02
  Attribute 0x06:
    0x01
";

      // When
      var deserialised = Node.Deserialise(binaryReader);
      var converted = (CabDataChunk<String>)_stringDumpNodeConverter.Convert(new Address(), deserialised).Single();
      var dumpedString = converted.Payload;

      // Then
      Assert.IsTrue(String.Equals(expectedString, dumpedString));
    }

    [TestMethod]
    public void Dumps_node_tree()
    {
      // Given
      byte[] packet =
      {
        0x00, 0x00, 0x00, 0x00,
          0x64, 0x00,
          0x09, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x33, 0x2E, 0x30, 0x2E, 0x31, 0x2E, 0x30,
          0x03, 0x00, 0x00, 0x00,
            0x02, 0x00,
            0x01,
          0x00, 0x00, 0x00, 0x00,
            0x64, 0x00,
            0x03, 0x00, 0x00, 0x00,
              0x03, 0x00,
              0x02,
            0x03, 0x00, 0x00, 0x00,
              0x04, 0x00,
              0x01,
            0x03, 0x00, 0x00, 0x00,
              0x05, 0x00,
              0x02,
          0xFF, 0xFF, 0xFF, 0xFF,
          0x03, 0x00, 0x00, 0x00,
            0x06, 0x00,
            0x01,
        0xFF, 0xFF, 0xFF, 0xFF,
      };
      var rxStream = new MemoryStream(packet);
      var binaryReader = new BinaryReader(rxStream, Encoding.ASCII);

      var expectedString =
@"Node 0x64:
  Attribute 0x01:
    0x33, 0x2E, 0x30, 0x2E, 0x31, 0x2E, 0x30
  Attribute 0x02:
    0x01
  Attribute 0x06:
    0x01
  Node 0x64:
    Attribute 0x03:
      0x02
    Attribute 0x04:
      0x01
    Attribute 0x05:
      0x02
";

      // When
      var deserialised = Node.Deserialise(binaryReader);
      var converted = (CabDataChunk<String>)_stringDumpNodeConverter.Convert(new Address(), deserialised).Single();
      var dumpedString = converted.Payload;

      // Then
      Assert.IsTrue(String.Equals(expectedString, dumpedString));
    }
  }
}