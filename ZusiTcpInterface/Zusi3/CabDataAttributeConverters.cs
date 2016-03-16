using System;

namespace ZusiTcpInterface.Zusi3
{
  public static class CabDataAttributeConverters
  {
    public static IProtocolChunk ConvertSingle(short id, byte[] payload)
    {
      return new CabDataChunk<float>(id, BitConverter.ToSingle(payload, 0));
    }

    public static IProtocolChunk ConvertBoolAsSingle(short id, byte[] payload)
    {
      return new CabDataChunk<bool>(id, BitConverter.ToSingle(payload, 0) != 0f);
    }
  }
}