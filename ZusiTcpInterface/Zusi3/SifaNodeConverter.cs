using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  internal class SifaNodeConverter : INodeConverter
  {
    private NodeConverter _dataConverter;

    private const short TypeId = 0x01;
    private const short PilotLightId = 0x02;
    private const short HornStateId = 0x03;
    private const short MainSwitchId = 0x04;
    private const short DisruptionOverrideId = 0x05;
    private const short AirCutoffValveId = 0x06;

    public SifaNodeConverter()
    {
      var attributeConverters = new Dictionary<short, Func<short, byte[], IProtocolChunk>>();

      attributeConverters[TypeId] = AttributeConverters.ConvertString;
      attributeConverters[PilotLightId] = AttributeConverters.ConvertBool;
      attributeConverters[HornStateId] = ConvertHornState;
      attributeConverters[MainSwitchId] = AttributeConverters.ConvertOneBasedBool;
      attributeConverters[DisruptionOverrideId] = AttributeConverters.ConvertOneBasedBool;
      attributeConverters[AirCutoffValveId] = AttributeConverters.ConvertOneBasedBool;

      _dataConverter = new NodeConverter{ ConversionFunctions = attributeConverters };
    }

    private static IProtocolChunk ConvertHornState(short id, byte[] payload)
    {
      return new CabDataChunk<SifaHornState>(id, (SifaHornState)payload.Single());
    }

    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      var fields = _dataConverter.Convert(node).Cast<CabDataChunkBase>();

      string type = null;
      bool? pilotLight = null, mainSwitch = null, disruptionOverride = null, airCutoffValve = null;
      SifaHornState? hornState = null;

      foreach (var field in fields)
      {
        switch (field.Id)
        {
          case TypeId:
            type = ((CabDataChunk<string>) field).Payload;
            break;

          case PilotLightId:
            pilotLight = ((CabDataChunk<bool>)field).Payload;
            break;

          case MainSwitchId:
            mainSwitch = ((CabDataChunk<bool>)field).Payload;
            break;

          case DisruptionOverrideId:
            disruptionOverride = ((CabDataChunk<bool>)field).Payload;
            break;

          case AirCutoffValveId:
            airCutoffValve = ((CabDataChunk<bool>)field).Payload;
            break;

          case HornStateId:
            hornState = ((CabDataChunk<SifaHornState>)field).Payload;
            break;

          default:
            throw new InvalidOperationException("Invalid Sifa packet received.");
        }
      }

      if(type == null)
        throw new InvalidOperationException("Sifa type was not set.");

      var sifaStatus = new SifaStatus(type, pilotLight.Value, mainSwitch.Value, hornState.Value, disruptionOverride.Value,
        airCutoffValve.Value);

      return new[]
      {
        new CabDataChunk<SifaStatus>(node.Id, sifaStatus),
      };
    }
  }
}