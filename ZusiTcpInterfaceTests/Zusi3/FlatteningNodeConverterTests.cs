using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Enums;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class FlatteningNodeConverterTests
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
      var converter = new FlatteningNodeConverter
      {
        ConversionFunctions =
        {
          [new Address(0x0A, 0x01)] = AttributeConverters.ConvertSingle,
          [new Address(0x0A, 0x1B)] = AttributeConverters.ConvertBoolAsSingle,
          [new Address(0x0A, 0x1C)] = AttributeConverters.ConvertBoolAsSingle
        }
      };


      // When
      var chunks = converter.Convert(new Address(), cabDataNode).Cast<DataChunkBase>().ToList();

      // Then
      var velocity = ((DataChunk<float>)chunks.Single(chunk => chunk.Address == new Address(0x0A, 0x01))).Payload;
      var pilotLightState = ((DataChunk<bool>)chunks.Single(chunk => chunk.Address == new Address(0x0A, 0x1B))).Payload;
      var otherPilotLightState = ((DataChunk<bool>)chunks.Single(chunk => chunk.Address == new Address(0x0A, 0x1C))).Payload;

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
      var cabDataNode = new Node(0x0A, subNode);

      var rootConverter = new RootNodeConverter();
      var cabDataConverter = new FlatteningNodeConverter()
      {
        ConversionFunctions =
        {
          [new Address(0x0A, 0x64, 0x01)] = AttributeConverters.ConvertString,
          [new Address(0x0A, 0x64, 0x02)] = AttributeConverters.ConvertEnumAsByte<StatusSifaHupe>
        }
      };

      rootConverter.SubNodeConverters[0x0A] = cabDataConverter;

      // When
      var chunks = rootConverter.Convert(cabDataNode).Cast<DataChunkBase>().ToDictionary(chunk => chunk.Address);

      // Then
      Assert.AreEqual(type, ((DataChunk<string>)chunks[new Address(0x0A, 0x64, 0x01)]).Payload);
      Assert.AreEqual(StatusSifaHupe.Warnung, ((DataChunk<StatusSifaHupe>)chunks[new Address(0x0A, 0x64, 0x02)]).Payload);
    }
  }
}                                                                                      
