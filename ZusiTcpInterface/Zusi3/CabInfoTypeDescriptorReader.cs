using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ZusiTcpInterface.Zusi3
{
  internal static class CabInfoTypeDescriptorReader
  {
    public static IEnumerable<CabInfoTypeDescriptor> ReadCommandsetFrom(Stream inputStream)
    {
      var commands = new List<CabInfoTypeDescriptor>();
      var streamReader = new StreamReader(inputStream);

      while (!streamReader.EndOfStream)
      {
        var line = streamReader.ReadLine();

        // ToDo: Make handling of syntax errors more graceful - e.g. w/ error message and continuation...
        var columns = line.Split(';');
        short id = short.Parse(columns[0].Split('x', 'X')[1], NumberStyles.HexNumber);
        string name = columns[1];
        string unit = columns[2];
        string converter = columns[3];
        string comment = (columns.Length == 5) ? columns[4] : "";

        commands.Add(new CabInfoTypeDescriptor(id, name, unit, converter, comment));
      }

      return commands;
    }
  }
}