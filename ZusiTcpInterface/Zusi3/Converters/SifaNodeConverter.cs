using System;
using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class SifaNodeConverter : INodeConverter
  {
    private readonly NodeConverter _dataConverter;

    private static readonly Address TypeId = new Address(0x01);
    private static readonly Address PilotLightId = new Address(0x02);
    private static readonly Address HornStateId = new Address(0x03);
    private static readonly Address MainSwitchId = new Address(0x04);
    private static readonly Address DisruptionOverrideId = new Address(0x05);
    private static readonly Address AirCutoffValveId = new Address(0x06);

    public SifaNodeConverter()
    {
      var attributeConverters = new Dictionary<short, Func<Address, byte[], IProtocolChunk>>();

      attributeConverters[TypeId] = AttributeConverters.ConvertString;
      attributeConverters[PilotLightId] = AttributeConverters.ConvertBool;
      attributeConverters[HornStateId] = AttributeConverters.ConvertEnumAsByte<SifaHornState>;
      attributeConverters[MainSwitchId] = AttributeConverters.ConvertOneBasedBool;
      attributeConverters[DisruptionOverrideId] = AttributeConverters.ConvertOneBasedBool;
      attributeConverters[AirCutoffValveId] = AttributeConverters.ConvertOneBasedBool;

      _dataConverter = new NodeConverter{ ConversionFunctions = attributeConverters };
    }

    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      var fields = _dataConverter.Convert(node).Cast<CabDataChunkBase>();

      string type = null;
      bool? pilotLight = null, mainSwitch = null, disruptionOverride = null, airCutoffValve = null;
      SifaHornState? hornState = null;

      foreach (var field in fields)
      {
        if (field.Id == TypeId)
        {
          type = ((CabDataChunk<string>) field).Payload;
        }
        else if (field.Id == PilotLightId)
        {
          pilotLight = ((CabDataChunk<bool>) field).Payload;
        }
        else if (field.Id == MainSwitchId)
        {
          mainSwitch = ((CabDataChunk<bool>) field).Payload;
        }
        else if (field.Id == DisruptionOverrideId)
        {
          disruptionOverride = ((CabDataChunk<bool>) field).Payload;
        }
        else if (field.Id == AirCutoffValveId)
        {
          airCutoffValve = ((CabDataChunk<bool>) field).Payload;
        }
        else if (field.Id == HornStateId)
        {
          hornState = ((CabDataChunk<SifaHornState>) field).Payload;
        }
        else
        {
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