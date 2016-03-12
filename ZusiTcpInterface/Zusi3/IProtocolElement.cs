using System.Collections.Generic;

namespace ZusiTcpInterface.Zusi3
{
  internal interface IProtocolElement
  {
    IEnumerable<byte> Serialise();
  }
}