﻿using System;
using System.Linq;
using System.Text;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal static class AttributeConverters
  {
    public static IProtocolChunk ConvertSingle(short id, byte[] payload)
    {
      return new CabDataChunk<float>(id, BitConverter.ToSingle(payload, 0));
    }

    public static IProtocolChunk ConvertBoolAsSingle(short id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, BitConverter.ToSingle(payload, 0) != 0f);
    }

    public static IProtocolChunk ConvertString(short id, byte[] payload)
    {
      return new CabDataChunk<string>(id, Encoding.ASCII.GetString(payload));
    }

    public static IProtocolChunk ConvertBool(short id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, payload.Single() != 0);
    }

    public static IProtocolChunk ConvertOneBasedBool(short id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, payload.Single() != 1);
    }

    public static IProtocolChunk ConvertEnumAsByte<T>(short id, byte[] payload)
    {
      var enumValue = (T) (object) payload.Single();

      return new CabDataChunk<T>(id, enumValue);
    }

    public static IProtocolChunk ConvertEnumAsShort<T>(short id, byte[] payload)
    {
      var enumValue = (T) (object) Convert.ToInt16(payload);

      return new CabDataChunk<T>(id, enumValue);
    }
  }
}