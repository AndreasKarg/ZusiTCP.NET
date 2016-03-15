using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace ZusiTcpInterface.Zusi3
{
  internal class Attribute : IProtocolElement
  {
    private readonly byte[] _payload;
    private readonly short _id;

    private static readonly LittleEndianBitConverter BitConverter = EndianBitConverter.Little;

    public short Id
    {
      get { return _id; }
    }

    public byte[] Payload
    {
      get { return _payload; }
    }

    public Attribute(short id, byte[] payload)
    {
      _id = id;
      _payload = payload;
    }

    public Attribute(short id, byte payload)
      : this(id, BitConverter.GetBytes(payload))
    {
    }

    public Attribute(short id, short payload)
      : this(id, BitConverter.GetBytes(payload))
    {
    }

    public Attribute(short id, string payload)
      : this(id, Encoding.ASCII.GetBytes(payload))
    {
    }

    public Attribute(short id, float payload)
      : this(id, BitConverter.GetBytes(payload))
    {
    }

    public void Serialise(BinaryWriter binaryWriter)
    {
      int length = _payload.Length + 2;
      binaryWriter.Write(length);
      binaryWriter.Write(_id);
      binaryWriter.Write(_payload);
    }

    [Pure]
    public static Attribute Deserialise(BinaryReader rxStream, int length)
    {
      //var length = BitConverter.ToInt32(rxStream.Read(4), 0);
      var id = rxStream.ReadInt16();

      var payload = new byte[length - 2];
      rxStream.Read(payload, 0, payload.Length);

      return new Attribute(id, payload);
    }
  }
}