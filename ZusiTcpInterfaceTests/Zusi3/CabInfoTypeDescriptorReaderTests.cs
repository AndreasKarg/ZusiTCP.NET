using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class CabInfoTypeDescriptorReaderTests
  {
    [TestMethod]
    public void Reads_flat_Xml_correctly()
    {
      // Given
      string commandsetXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition>
  <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0002"" name=""Druck Hauptluftleitung"" unit=""bar"" converter=""Single"" />
  <Attribute id=""0x0003"" name=""Druck Bremszylinder"" unit=""bar"" converter=""Single"" />
  <Attribute id=""0004"" name=""Druck Hauptluftbehälter"" unit=""bar"" converter=""Single"" comment=""Mit Sauce""/>
  <Attribute id=""0005"" name=""Luftpresser läuft"" unit=""aus/an"" converter=""BoolAsSingle"" />
  <Attribute id=""0006"" name=""Luftstrom Fbv"" unit=""-1...0...1"" converter=""Fail"" />
  <Attribute id=""0007"" name=""Luftstrom Zbv"" unit=""-1...0...1"" converter=""Single"" />
  <Attribute id=""0008"" name=""Lüfter an"" unit=""aus/an"" converter=""BoolAsSingle"" />
</ProtocolDefinition>";

      var expectedCommands = new List<CabInfoAttributeDescriptor>
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
        new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
        new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
        new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
        new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle"),
      };

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When
      var descriptors = CabInfoTypeDescriptorReader.ReadCommandsetFrom(inputStream);

      // Then
      foreach (var expectedCommand in expectedCommands)
      {
        Assert.IsTrue(descriptors.Contains(expectedCommand, EqualityComparer<CabInfoAttributeDescriptor>.Default));
      }
    }

    [TestMethod]
    public void Reads_nested_Xml_correctly()
    {
      // Given
      string commandsetXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition>
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

      var expectedCommands = new List<CabInfoDescriptorBase>
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
        new CabInfoNodeDescriptor(123, "Test", new List<CabInfoAttributeDescriptor>{
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(153, "Test2", new List<CabInfoAttributeDescriptor>{
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })
      };

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetXml.ToCharArray()));

      // When
      var commands = CabInfoTypeDescriptorReader.ReadCommandsetFrom(inputStream).ToList();

      // Then
      foreach (var expectedCommand in expectedCommands)
      {
        Assert.IsTrue(commands.Contains(expectedCommand));
      }
    }
  }
}