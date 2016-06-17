using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class SifaNodeConverterTests
  {
    private readonly SifaNodeConverter _sifaConverter = new SifaNodeConverter();

    [TestMethod]
    public void Deserialises_well_formed_packet()
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

      // When
      var deserialised = Node.Deserialise(binaryReader);
      var converted = (CabDataChunk<SifaStatus>)_sifaConverter.Convert(deserialised).Single();
      var sifaStatus = converted.Payload;

      // Then
      Assert.AreEqual("3.0.1.0", sifaStatus.Type);
      Assert.AreEqual(true, sifaStatus.PilotLightOn);
      Assert.AreEqual(SifaHornState.AutomaticBraking, sifaStatus.HornState);
      Assert.AreEqual(false, sifaStatus.MainSwitchEnabled);
      Assert.AreEqual(true, sifaStatus.DisruptionOverrideSwitchEnabled);
      Assert.AreEqual(false, sifaStatus.AirCutoffValveOpen);
    }
  }
}