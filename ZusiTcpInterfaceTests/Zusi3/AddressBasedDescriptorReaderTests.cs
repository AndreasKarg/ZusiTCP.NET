using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestExtensions;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class AddressBasedDescriptorReaderTests : BaseTest
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
        new AddressBasedAttributeDescriptor(new Address(0x01), "Geschwindigkeit", "m/s", "Single"),
        new AddressBasedAttributeDescriptor(new Address(0x02), "Druck Hauptluftleitung", "bar", "Single"),
        new AddressBasedAttributeDescriptor(new Address(0x03), "Druck Bremszylinder", "bar", "Single"),
        new AddressBasedAttributeDescriptor(new Address(0x04), "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new AddressBasedAttributeDescriptor(new Address(0x05), "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new AddressBasedAttributeDescriptor(new Address(0x06), "Luftstrom Fbv", "-1...0...1", "Fail"),
        new AddressBasedAttributeDescriptor(new Address(0x07), "Luftstrom Zbv", "-1...0...1", "Single"),
        new AddressBasedAttributeDescriptor(new Address(0x08), "Lüfter an", "aus/an", "BoolAsSingle"),
      };
      var expectedDescriptors = new AddressBasedDescriptorCollection<AddressBasedAttributeDescriptor>(expectedDescriptorList);

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));
      var baseAddress = new Address();

      // When
      var descriptorList = AddressBasedDescriptorReader.ReadCommandsetFrom(inputStream, baseAddress);
      var descriptors = new AddressBasedDescriptorCollection<AddressBasedAttributeDescriptor>(descriptorList);

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
        new AddressBasedAttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "m/s", "Single"),
        new AddressBasedAttributeDescriptor(new CabInfoAddress(0x02), "Druck Hauptluftleitung", "bar", "Single"),

          new AddressBasedAttributeDescriptor(new CabInfoAddress(0x123, 0x03), "Druck Bremszylinder", "bar", "Single"),
          new AddressBasedAttributeDescriptor(new CabInfoAddress(0x123, 0x04), "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new AddressBasedAttributeDescriptor(new CabInfoAddress(0x123, 0x05), "Luftpresser läuft", "aus/an", "BoolAsSingle"),

          new AddressBasedAttributeDescriptor(new CabInfoAddress(0x153, 0x06), "Luftstrom Fbv", "-1...0...1", "Fail"),
          new AddressBasedAttributeDescriptor(new CabInfoAddress(0x153, 0x07), "Luftstrom Zbv", "-1...0...1", "Single"),
          new AddressBasedAttributeDescriptor(new CabInfoAddress(0x153, 0x08), "Lüfter an", "aus/an", "BoolAsSingle")
      };

      var expectedDescriptors = new AddressBasedDescriptorCollection<AddressBasedAttributeDescriptor>(expectedDescriptorList);

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));
      var baseAddress = new CabInfoAddress();

      // When
      var descriptorList = AddressBasedDescriptorReader.ReadCommandsetFrom(inputStream, baseAddress);
      var descriptors = new AddressBasedDescriptorCollection<AddressBasedAttributeDescriptor>(descriptorList);


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
      var descriptorList = AddressBasedDescriptorReader.ReadCommandsetFrom(inputStream, new Address());

      // When - Throws
      Assert.Throws<InvalidDescriptorException>(() => new AddressBasedDescriptorCollection<AddressBasedAttributeDescriptor>(descriptorList));
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
      var descriptorList = AddressBasedDescriptorReader.ReadCommandsetFrom(inputStream, new Address());

      // When - Throws
      Assert.Throws<InvalidDescriptorException>(() => new AddressBasedDescriptorCollection<AddressBasedAttributeDescriptor>(descriptorList));
    }
  }
}
