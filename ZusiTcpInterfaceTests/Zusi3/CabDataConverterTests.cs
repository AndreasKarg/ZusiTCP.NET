using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3;
using Attribute = ZusiTcpInterface.Zusi3.Attribute;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class CabDataConverterTests
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
      var converter = new CabDataConverter();

      converter.ConversionFunctions[0x01] = CabDataAttributeConverters.ConvertSingle;
      converter.ConversionFunctions[0x1B] = CabDataAttributeConverters.ConvertBoolAsSingle;
      converter.ConversionFunctions[0x1C] = CabDataAttributeConverters.ConvertBoolAsSingle;

      // When
      var chunks = converter.Convert(cabDataNode).Cast<CabDataChunkBase>().ToList();

      // Then
      var velocity = ((CabDataChunk<float>)chunks.Single(chunk => chunk.Id == 0x01)).Payload;
      var pilotLightState = ((CabDataChunk<bool>)chunks.Single(chunk => chunk.Id == 0x1B)).Payload;
      var otherPilotLightState = ((CabDataChunk<bool>)chunks.Single(chunk => chunk.Id == 0x1C)).Payload;

      Assert.AreEqual(expectedVelocity, velocity);
      Assert.AreEqual(expectedPilotLightState, pilotLightState);
      Assert.AreEqual(expectedOtherPilotLightState, otherPilotLightState);
    }
  }
}