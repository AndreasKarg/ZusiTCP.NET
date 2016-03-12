using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;

namespace ZusiTcpInterface.Zusi3
{
  internal class Attribute : IProtocolElement
  {
    private readonly byte[] _content;
    private static readonly LittleEndianBitConverter BitConverter = EndianBitConverter.Little;

    public Attribute(short attributeId, byte[] payload)
    {
      _content = BitConverter.GetBytes(attributeId)
                .Concat(payload).ToArray();
    }

    public Attribute(short attributeId, byte payload)
      : this(attributeId, BitConverter.GetBytes(payload))
    {
    }

    public Attribute(short attributeId, short payload)
      : this(attributeId, BitConverter.GetBytes(payload))
    {
    }

    public Attribute(short attributeId, string payload)
      : this(attributeId, Encoding.ASCII.GetBytes(payload))
    {
    }

    public IEnumerable<byte> Serialise()
    {
      var lengthPrefix = BitConverter.GetBytes(_content.Length);

      return lengthPrefix
            .Concat(_content);
    }
  }
}