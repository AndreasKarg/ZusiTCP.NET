using System;
using System.Linq;
using System.Text;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal static class AttributeConverters
  {
    public static IProtocolChunk ConvertSingle(Address id, byte[] payload)
    {
      return new CabDataChunk<float>(id, BitConverter.ToSingle(payload, 0));
    }

    public static IProtocolChunk ConvertBoolAsSingle(Address id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, BitConverter.ToSingle(payload, 0) != 0f);
    }

    public static IProtocolChunk ConvertString(Address id, byte[] payload)
    {
      return new CabDataChunk<string>(id, Encoding.ASCII.GetString(payload));
    }

    public static IProtocolChunk ConvertBoolAsByte(Address id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, payload.Single() != 0);
    }

    public static IProtocolChunk ConvertEnumAsByte<T>(Address id, byte[] payload)
    {
      var enumValue = CastToEnum<T>(payload.Single());

      return new CabDataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertEnumAsShort<T>(Address id, byte[] payload)
    {
      var enumValue = CastToEnum<T>(BitConverter.ToInt16(payload, 0));

      return new CabDataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertShort(Address id, byte[] payload)
    {
      return new CabDataChunk<short>(id, BitConverter.ToInt16(payload, 0));
    }

    private static T CastToEnum<T>(int value)
    {
      return (T) (object) value;
    }
  }
}