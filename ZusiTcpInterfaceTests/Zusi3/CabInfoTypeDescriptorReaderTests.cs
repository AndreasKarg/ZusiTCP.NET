using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class CabInfoTypeDescriptorReaderTests
  {
    [TestMethod]
    public void ReadsCsvCorrectly()
    {
      // Given
      string commandsetCsv =
@"0x0001;Geschwindigkeit;m/s;Single
0x0002;Druck Hauptluftleitung;bar;Single
0x0003;Druck Bremszylinder;bar;Single
0x0004;Druck Hauptluftbehälter;bar;Single;Mit Sauce
0x0005;Luftpresser läuft;aus/an;BoolAsSingle
0x0006;Luftstrom Fvb;-1...0...1;Fail
0x0007;Luftstrom Zbv;-1...0...1;Single
0x0008;Lüfter an;aus/an;BoolAsSingle";

      var expectedCommands = new List<CabInfoTypeDescriptor>
      {
        new CabInfoTypeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoTypeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
        new CabInfoTypeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
        new CabInfoTypeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
        new CabInfoTypeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        new CabInfoTypeDescriptor(0x06, "Luftstrom Fvb", "-1...0...1", "Fail"),
        new CabInfoTypeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
        new CabInfoTypeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle"),
      };

      var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(commandsetCsv.ToCharArray()));

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