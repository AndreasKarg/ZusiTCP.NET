using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3.Converters
{
  internal class StringDumpNodeConverter : INodeConverter
  {
    public IEnumerable<IProtocolChunk> Convert(Node node)
    {
      var dump = node.DumpToStrings().Aggregate(new StringBuilder(), (sb, line) => sb.AppendLine(line)).ToString();
      yield return new CabDataChunk<String>(node.Id, dump);
    }
  }
}