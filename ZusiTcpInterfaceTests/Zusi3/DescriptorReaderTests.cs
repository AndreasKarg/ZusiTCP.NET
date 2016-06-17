using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestExtensions;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class DescriptorReaderTests : BaseTest
  {
    [TestMethod]
    public void Reads_flat_Xml_correctly()
    {
      // Given
      string commandsetXml =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition xmlns=""ZusiTcpInterface/CabInfoTypes"">
  <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0002"" name=""Druck Hauptluftleitung"" unit=""bar"" converter=""Single"" />
  <Attribute id=""0x0003"" name=""Druck Bremszylinder"" unit=""bar"" converter=""Single"" />
  <Attribute id=""0004"" name=""Druck Hauptluftbehälter"" unit=""bar"" converter=""Single"" comment=""Mit Sauce""/>
  <Attribute id=""0005"" name=""Luftpresser läuft"" unit=""aus/an"" converter=""BoolAsSingle"" />
  <Attribute id=""0006"" name=""Luftstrom Fbv"" unit=""-1...0...1"" converter=""Fail"" />
  <Attribute id=""0007"" name=""Luftstrom Zbv"" unit=""-1...0...1"" converter=""Single"" />
  <Attribute id=""0008"" name=""Lüfter an"" unit=""aus/an"" converter=""BoolAsSingle"" />
</ProtocolDefinition>";

      var expectedDescriptors = new NodeDescriptor(0, "Root", new[]
      {
        new AttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle"),
      });

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When
      var descriptors = DescriptorReader.ReadCommandsetFrom(inputStream);

      // Then
      Assert.AreEqual(expectedDescriptors, descriptors);
    }

    [TestMethod]
    public void Reads_nested_Xml_correctly()
    {
      // Given
      string commandsetXml =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition xmlns=""ZusiTcpInterface/CabInfoTypes"">
  <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0002"" name=""Druck Hauptluftleitung"" unit=""bar"" converter=""Single"" />
  <Node id=""123"" name=""Test"" comment=""Lalala"">
    <Attribute id=""0x0003"" name=""Druck Bremszylinder"" unit=""bar"" converter=""Single"" />
    <Attribute id=""0004"" name=""Druck Hauptluftbehälter"" unit=""bar"" converter=""Single"" comment=""Mit Sauce""/>
    <Attribute id=""0005"" name=""Luftpresser läuft"" unit=""aus/an"" converter=""BoolAsSingle"" />
  </Node>
  <Node id=""153"" name=""Test2"">
    <Attribute id=""0006"" name=""Luftstrom Fbv"" unit=""-1...0...1"" converter=""Fail"" />
    <Attribute id=""0007"" name=""Luftstrom Zbv"" unit=""-1...0...1"" converter=""Single"" />
    <Attribute id=""0008"" name=""Lüfter an"" unit=""aus/an"" converter=""BoolAsSingle"" />
  </Node>
</ProtocolDefinition>";

      var expectedAttributes = new[]
      {
        new AttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var expectedNodes = new[]
      {
        new NodeDescriptor(0x123, "Test", new[]
        {
          new AttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new AttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new AttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new NodeDescriptor(0x153, "Test2", new[]
        {
          new AttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new AttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new AttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })
      };

      var expectedDescriptors = new NodeDescriptor(0, "Root", expectedAttributes, expectedNodes);

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When
      var descriptors = DescriptorReader.ReadCommandsetFrom(inputStream);

      // Then
      Assert.AreEqual(expectedDescriptors, descriptors);
    }

    [TestMethod]
    public void Throws_exception_on_duplicate_attribute_name()
    {
      // Given
      string commandsetXml =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition xmlns=""ZusiTcpInterface/CabInfoTypes"">
  <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0002"" name=""Geschwindigkeit"" unit=""bar"" converter=""Single"" />
</ProtocolDefinition>";
      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When - Throws
      Assert.Throws<InvalidDescriptorException>(() => DescriptorReader.ReadCommandsetFrom(inputStream));
    }

    [TestMethod]
    public void Throws_exception_on_duplicate_attribute_id()
    {
      // Given
      string commandsetXml =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition xmlns=""ZusiTcpInterface/CabInfoTypes"">
  <Attribute id=""0001"" name=""Foo"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0001"" name=""Bar"" unit=""bar"" converter=""Single"" />
</ProtocolDefinition>";
      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When - Throws
      Assert.Throws<InvalidDescriptorException>(() => DescriptorReader.ReadCommandsetFrom(inputStream));
    }
  }
}