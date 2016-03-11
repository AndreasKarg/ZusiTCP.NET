using System.Collections.Generic;

namespace ZusiTcpInterface.Zusi2
{
  internal interface IWritableStream
  {
    void Write(IEnumerable<byte> data);
  }
}