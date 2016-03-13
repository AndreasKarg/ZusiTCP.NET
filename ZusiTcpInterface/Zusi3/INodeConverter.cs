using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ZusiTcpInterface.Zusi3
{
  internal interface INodeConverter
  {
    [Pure]
    IEnumerable<ProtocolChunk> Convert(Node node);
  }
}