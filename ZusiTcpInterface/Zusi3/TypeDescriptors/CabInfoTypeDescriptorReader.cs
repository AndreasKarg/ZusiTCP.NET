using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  internal static class CabInfoTypeDescriptorReader
  {
    public static IEnumerable<CabInfoTypeDescriptor> ReadCommandsetFrom(Stream inputStream)
    {
      var root = XElement.Load(inputStream);

      return root.Elements().Select(ConvertToDescriptor);
    }

    private static CabInfoTypeDescriptor ConvertToDescriptor(XElement arg)
    {
      var attributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(attributes["id"], 16);
      var name = attributes["name"];
      var converter = attributes["converter"];

      // Optional attributes
      string unit = attributes.ContainsKey("unit") ? attributes["unit"] : String.Empty;
      string comment = attributes.ContainsKey("comment") ? attributes["comment"] : String.Empty;

      return new CabInfoTypeDescriptor(id, name, unit, converter, comment);
    }
  }
}