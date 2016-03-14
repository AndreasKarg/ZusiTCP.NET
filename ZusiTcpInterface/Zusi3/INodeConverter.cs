using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ZusiTcpInterface.Zusi3
{
  internal interface INodeConverter
  {
    [Pure]
    IEnumerable<IProtocolChunk> Convert(Node node);
  }
}