using System.Linq;
using MiscUtil.Conversion;

namespace ZusiTcpInterface
{
  internal struct AckHelloPacket
  {
    private const int HeaderLength = 4;
    private const int InstructionLength = 2;
    private const int AckLength = 1;

    private readonly bool _acknowledged;

    private AckHelloPacket(bool acknowledged) : this()
    {
      _acknowledged = acknowledged;
    }

    public bool Acknowledged
    {
      get { return _acknowledged; }
    }

    public static AckHelloPacket Deserialise(IReadableStream rxStream)
    {
      var bitConverter = EndianBitConverter.Little;
      var packetLength = bitConverter.ToInt32(rxStream.Read(HeaderLength), 0);
      var instruction = bitConverter.ToInt16(rxStream.Read(InstructionLength), 0);
      var ack = rxStream.Read(AckLength).Single();

      return new AckHelloPacket(ack == 0);
    }
  }
}