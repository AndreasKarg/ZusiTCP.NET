using System.IO;

namespace ZusiTcpInterface.Zusi2
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

    public static AckHelloPacket Deserialise(BinaryReader binaryReader)
    {
      var packetLength = binaryReader.ReadInt32();
      var instruction = binaryReader.ReadInt16();
      var ack = binaryReader.ReadByte();

      return new AckHelloPacket(ack == 0);
    }
  }
}