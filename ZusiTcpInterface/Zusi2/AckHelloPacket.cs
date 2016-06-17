using System.IO;

namespace ZusiTcpInterface.Zusi2
{
  internal struct AckHelloPacket
  {
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