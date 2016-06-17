using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal interface INodeConverter
  {
    [Pure]
    IEnumerable<IProtocolChunk> Convert(Node node);
  }
}