using System.Collections.Generic;

namespace ZusiTcpInterface
{
  internal interface IWritableStream
  {
    void Write(IEnumerable<byte> data);
  }
}