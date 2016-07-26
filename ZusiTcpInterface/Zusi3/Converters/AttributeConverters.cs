using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3.Enums;
using ZusiTcpInterface.Zusi3.Enums.Lzb;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal static class AttributeConverters
  {
    private static readonly Dictionary<string, Func<Address, byte[], IProtocolChunk>> ConverterMap =
      new Dictionary<string, Func<Address, byte[], IProtocolChunk>>(StringComparer.InvariantCultureIgnoreCase)
      {
        {"single", ConvertSingle},
        {"boolassingle", ConvertBoolAsSingle},
        {"boolasbyte", ConvertBoolAsByte},
        {"string", ConvertString},
        {"zugart", ConvertEnumAsShort<Zugart>},
        {"switchstate", ConvertEnumAsByte<SwitchState>},
        {"aktivezugdaten", ConvertEnumAsShort<AktiveZugdaten>},
        {"statussifahupe", ConvertEnumAsByte<StatusSifaHupe>},
        {"zustandzugsicherung", ConvertEnumAsShort<ZustandZugsicherung>},
        {"grundzwangsbremsung", ConvertEnumAsShort<GrundZwangsbremsung>},
        {"lzbzustand", ConvertEnumAsShort<LzbZustand>},
        {"statuslzbuebertragungsausfall", ConvertEnumAsShort<StatusLzbUebertragungsausfall>},
        {"indusihupe", ConvertEnumAsByte<IndusiHupe>},
        {"zusatzinfomelderbild", ConvertEnumAsByte<ZusatzinfoMelderbild>},
        {"pilotlightstate", ConvertEnumAsByte<PilotLightState>},
        {"statusendeverfahren", ConvertEnumAsByte<StatusEndeVerfahren>},
        {"statusauftrag", ConvertEnumAsByte<StatusAuftrag>},
        {"statusvorsichtauftrag", ConvertEnumAsByte<StatusVorsichtauftrag>},
        {"statusnothalt", ConvertEnumAsByte<StatusLzbNothalt>},
        {"statusrechnerausfall", ConvertEnumAsByte<StatusRechnerausfall>},
        {"statuselauftrag", ConvertEnumAsByte<StatusElAuftrag>},
        {"short", ConvertShort},
        {"fail", (s, bytes) => { throw new NotSupportedException("Unsupported data type received"); }}
      };

    public static Dictionary<Address, Func<Address, byte[], IProtocolChunk>> MapToDescriptors(IEnumerable<AttributeDescriptor> attributeDescriptors)
    {
      var mappedConverters = new Dictionary<Address, Func<Address, byte[], IProtocolChunk>>();

      foreach (var descriptor in attributeDescriptors)
      {
        try
        {
          mappedConverters.Add(descriptor.Address, ConverterMap[descriptor.Type]);
        }
        catch (KeyNotFoundException e)
        {
          throw new InvalidDescriptorException(
            String.Format("Could not found converter for type '{0}', used in descriptor 0x{1:x4} - {2}.", descriptor.Type, descriptor.Address, descriptor.Name), e);
        }
      }

      return mappedConverters;
    }

    public static IProtocolChunk ConvertSingle(Address id, byte[] payload)
    {
      return new DataChunk<float>(id, BitConverter.ToSingle(payload, 0));
    }

    public static IProtocolChunk ConvertBoolAsSingle(Address id, byte[] payload)
    {
      return new DataChunk<bool>(id, BitConverter.ToSingle(payload, 0) != 0f);
    }

    public static IProtocolChunk ConvertString(Address id, byte[] payload)
    {
      return new DataChunk<string>(id, Encoding.ASCII.GetString(payload));
    }

    public static IProtocolChunk ConvertBoolAsByte(Address id, byte[] payload)
    {
      return new DataChunk<bool>(id, payload.Single() != 0);
    }

    public static IProtocolChunk ConvertEnumAsByte<T>(Address id, byte[] payload)
    {
      var enumValue = CastToEnum<T>(payload.Single());

      return new DataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertEnumAsShort<T>(Address id, byte[] payload)
    {
      var enumValue = CastToEnum<T>(BitConverter.ToInt16(payload, 0));

      return new DataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertShort(Address id, byte[] payload)
    {
      return new DataChunk<short>(id, BitConverter.ToInt16(payload, 0));
    }

    private static T CastToEnum<T>(int value)
    {
      return (T) (object) value;
    }
  }
}