using System.IO;

namespace ZusiTcpInterface.Zusi3.DOM
{
  internal interface IProtocolElement
  {
    void Serialise(BinaryWriter binaryWriter);
  }
}