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
      var chunks = converter.Convert(new Address(), cabDataNode).Cast<CabDataChunkBase>().ToList();

      // Then
      var velocity = ((CabDataChunk<float>)chunks.Single(chunk => chunk.Address == new Address(0x01))).Payload;
      var pilotLightState = ((CabDataChunk<bool>)chunks.Single(chunk => chunk.Address == new Address(0x1B))).Payload;
      var otherPilotLightState = ((CabDataChunk<bool>)chunks.Single(chunk => chunk.Address == new Address(0x1C))).Payload;

      Assert.AreEqual(expectedVelocity, velocity);
      Assert.AreEqual(expectedPilotLightState, pilotLightState);
      Assert.AreEqual(expectedOtherPilotLightState, otherPilotLightState);
    }

    [TestMethod]
    public void Converts_child_node_correctly()
    {
      // Given
      var type = "3.0.1.0";

      var attributes = new Dictionary<short, Attribute>
      {
        { 0x01, new Attribute(0x01, type) },
        { 0x02, new Attribute(0x02, (byte)0x01) },
      };

      var subNode = new Node(0x64, attributes);
      var rootNode = new Node(0x0A, subNode);

      var rootConverter = new NodeConverter();

      var subNodeConverter = new NodeConverter(new Address(0x64));
      subNodeConverter.ConversionFunctions[0x01] = AttributeConverters.ConvertString;
      subNodeConverter.ConversionFunctions[0x02] = AttributeConverters.ConvertEnumAsByte<SifaHornState>;

      rootConverter.SubNodeConverters[0x64] = subNodeConverter;

      // When
      var chunks = rootConverter.Convert(new Address(), rootNode).Cast<CabDataChunkBase>().ToDictionary(chunk => chunk.Address);

      // Then
      Assert.AreEqual(type, ((CabDataChunk<string>)chunks[new Address(0x64, 0x01)]).Payload);
      Assert.AreEqual(SifaHornState.Warning, ((CabDataChunk<SifaHornState>)chunks[new Address(0x64, 0x02)]).Payload);
    }
  }
}                                                                                      