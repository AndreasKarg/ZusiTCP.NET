using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ZusiTcpInterface.DOM;

namespace ZusiTcpInterface.Converters
{
  internal interface INodeConverter
  {
    [Pure]
    IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node node);
  }
}