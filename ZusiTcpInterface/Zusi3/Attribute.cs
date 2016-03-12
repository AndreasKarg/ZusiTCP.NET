using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;
using ZusiTcpInterface.Common;

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

    public IEnumerable<byte> Serialise()
    {
      var content = BitConverter.GetBytes(_id)
                   .Concat(_payload).ToArray();
      var lengthPrefix = BitConverter.GetBytes(content.Length);

      return lengthPrefix
            .Concat(content);
    }

    public static Attribute Deserialise(IReadableStream rxStream)
    {
      var length = BitConverter.ToInt32(rxStream.Read(4), 0);
      var id = BitConverter.ToInt16(rxStream.Read(2), 0);

      var payload = rxStream.Read(length - 2);

      return new Attribute(id, payload);
    }
  }
}