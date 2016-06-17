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
    public void ReadsXmlCorrectly()
    {
      // Given
      string commandsetXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ProtocolDefinition>
  <Attribute id=""0001"" name=""Geschwindigkeit"" unit=""m/s"" converter=""Single"" />
  <Attribute id=""0002"" name=""Druck Hauptluftleitung"" unit=""bar"" converter=""Single"" />
  <Attribute id=""0003"" name=""Druck Bremszylinder"" unit=""bar"" converter=""Single"" />
  <Attribute id=""0004"" name=""Druck Hauptluftbehälter"" unit=""bar"" converter=""Single"" comment=""Mit Sauce""/>
  <Attribute id=""0005"" name=""Luftpresser läuft"" unit=""aus/an"" converter=""BoolAsSingle"" />
  <Attribute id=""0006"" name=""Luftstrom Fbv"" unit=""-1...0...1"" converter=""Fail"" />
  <Attribute id=""0007"" name=""Luftstrom Zbv"" unit=""-1...0...1"" converter=""Single"" />
  <Attribute id=""0008"" name=""Lüfter an"" unit=""aus/an"" converter=""BoolAsSingle"" />
</ProtocolDefinition>";

      var expectedCommands = new List<CabInfoTypeDescriptor>
      {
        new CabInfoTypeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoTypeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
        new CabInfoTypeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
        new CabInfoTypeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new CabInfoTypeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new CabInfoTypeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
        new CabInfoTypeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
        new CabInfoTypeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle"),
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