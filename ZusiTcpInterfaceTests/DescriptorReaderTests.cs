using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestExtensions;
using System.IO;
using System.Text;
using ZusiTcpInterface;
using ZusiTcpInterface.TypeDescriptors;

namespace ZusiTcpInterfaceTests
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

      var expectedDescriptorList = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x03), "Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x04), "Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x05), "Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AttributeDescriptor(new CabInfoAddress(0x06), "Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x07), "Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x08), "Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle"),
      };
      var expectedDescriptors = new DescriptorCollection(expectedDescriptorList);

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When
      var descriptorList = DescriptorReader.ReadCommandsetFrom(inputStream);
      var descriptors = new DescriptorCollection(descriptorList);

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

      var expectedDescriptorList = new[]
      {
        new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "Druck Hauptluftleitung", "bar", "Single"),

        new AttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Test:Druck Bremszylinder", "Druck Bremszylinder", "bar", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Test:Druck Hauptluftbehälter", "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Test:Luftpresser läuft", "Luftpresser läuft", "aus/an", "BoolAsSingle"),

        new AttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Test2:Luftstrom Fbv", "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Test2:Luftstrom Zbv", "Luftstrom Zbv", "-1...0...1", "Single"),
        new AttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Test2:Lüfter an", "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var expectedDescriptors = new DescriptorCollection(expectedDescriptorList);

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When
      var descriptorList = DescriptorReader.ReadCommandsetFrom(inputStream);
      var descriptors = new DescriptorCollection(descriptorList);

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
      var descriptorList = DescriptorReader.ReadCommandsetFrom(inputStream);

      // When - Throws
      Assert.Throws<InvalidDescriptorException>(() => new DescriptorCollection(descriptorList));
    }

    [TestMethod]
    public void Throws_exception_on_duplicate_attribute_address()
    {
      // Given
      string commandsetXml =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition xmlns=""ZusiTcpInterface/CabInfoTypes"">
  <Attribute id=""0001"" name=""Foo"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0001"" name=""Bar"" unit=""bar"" converter=""Single"" />
</ProtocolDefinition>";
      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));
      var descriptorList = DescriptorReader.ReadCommandsetFrom(inputStream);

      // When - Throws
      Assert.Throws<InvalidDescriptorException>(() => new DescriptorCollection(descriptorList));
    }

    [TestMethod]
    public void Uses_qualified_name_to_find_nested_descriptors()
    {
      // Given
      string commandsetXml =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition xmlns=""ZusiTcpInterface/CabInfoTypes"">
  <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  <Node id=""123"" name=""Test"" comment=""Lalala"">
    <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  </Node>
</ProtocolDefinition>";
      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));
      var descriptorList = DescriptorReader.ReadCommandsetFrom(inputStream);

      var outerDescriptor = new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "m/s", "Single");
      var innerDescriptor = new AttributeDescriptor(new CabInfoAddress(0x123, 0x01), "Test:Geschwindigkeit", "Geschwindigkeit", "m/s", "Single");

      // When
      var descriptorCollection = new DescriptorCollection(descriptorList);

      // Then
      Assert.AreEqual(outerDescriptor, descriptorCollection["Geschwindigkeit"]);
      Assert.AreEqual(innerDescriptor, descriptorCollection["Test:Geschwindigkeit"]);
    }
  }
}