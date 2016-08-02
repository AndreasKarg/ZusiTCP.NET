using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZusiTcpInterface.DOM;

namespace ZusiTcpInterface.Converters
{
  internal class StringDumpNodeConverter : INodeConverter
  {
    public IEnumerable<IProtocolChunk> Convert(Address baseAddress, Node node)
    {
      var dump = node.DumpToStrings().Aggregate(new StringBuilder(), (sb, line) => sb.AppendLine(line)).ToString();
      yield return new DataChunk<String>(baseAddress.Concat(node.Id), dump);
    }
  }
}