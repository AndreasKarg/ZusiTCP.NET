using System.IO;

namespace ZusiTcpInterface.Zusi3
{
  internal interface IProtocolElement
  {
    void Serialise(BinaryWriter binaryWriter);
  }
}