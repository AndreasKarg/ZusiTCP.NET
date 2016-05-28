using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;
using Attribute = ZusiTcpInterface.Zusi3.DOM.Attribute;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class NodeConverterTests
  {
    [TestMethod]
    public void Converts_node_correctly()
    {
      // Given
      var expectedVelocity = 11.83f;
      var expectedPilotLightState = false;
      var expectedOtherPilotLightState = true;

      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, expectedVelocity) },
        { 0x1B, new Attribute(0x1B, 0f) },
        { 0x1C, new Attribute(0x1C, 1f) }
      };

      var cabDataNode = new Node(0x0A, attributes);
      var converter = new NodeConverter();

      converter.ConversionFunctions[0x01] = AttributeConverters.ConvertSingle;
      converter.ConversionFunctions[0x1B] = AttributeConverters.ConvertBoolAsSingle;
      converter.ConversionFunctions[0x1C] = AttributeConverters.ConvertBoolAsSingle;

      // When
      var chunks = converter.Convert(cabDataNode).Cast<CabDataChunkBase>().ToList();

      // Then
      var velocity = ((CabDataChunk<float>)chunks.Single(chunk => chunk.Id == new Address(0x01))).Payload;
      var pilotLightState = ((CabDataChunk<bool>)chunks.Single(chunk => chunk.Id == new Address(0x1B))).Payload;
      var otherPilotLightState = ((CabDataChunk<bool>)chunks.Single(chunk => chunk.Id == new Address(0x1C)).Payload;

      Assert.AreEqual(expectedVelocity, velocity);
      Assert.AreEqual(expectedPilotLightState, pilotLightState);
      Assert.AreEqual(expectedOtherPilotLightState, otherPilotLightState);
    }

    [TestMethod]
    public void Converts_child_node_correctly()
    {
      // Given
      Assert.Inconclusive("Test does not make sense at the moment.");
        /*
      var type = "3.0.1.0";
      var expectedSifaStatus = new SifaStatus(type, true, false, SifaHornState.AutomaticBraking, true, false);

      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, type) },
        { 0x02, new Attribute(0x02, (byte)0x01) },
        { 0x03, new Attribute(0x03, (byte)0x02) },
        { 0x04, new Attribute(0x04, (byte)0x01) },
        { 0x05, new Attribute(0x05, (byte)0x02) },
        { 0x06, new Attribute(0x06, (byte)0x01) },
      };

      var sifaNode = new Node(0x64, attributes);
      var cabDataNode = new Node(0x0A, sifaNode);

      var converter = new NodeConverter();

      converter.SubNodeConverters[0x64] = new SifaNodeConverter();

      // When
      var actualSifaStatus = converter.Convert(cabDataNode).Cast<CabDataChunk<SifaStatus>>().Single();

      // Then
      Assert.AreEqual(expectedSifaStatus, actualSifaStatus.Payload);
      */
    }
  }
}