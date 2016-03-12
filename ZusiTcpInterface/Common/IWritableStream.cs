using System.Collections.Generic;

namespace ZusiTcpInterface.Common
{
  internal interface IWritableStream
  {
    void Write(IEnumerable<byte> data);
  }
}