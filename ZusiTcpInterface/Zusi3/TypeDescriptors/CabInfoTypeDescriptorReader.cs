using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  internal static class CabInfoTypeDescriptorReader
  {
    public static DescriptorCollection ReadCommandsetFrom(Stream inputStream)
    {
      var root = XElement.Load(inputStream);

      return new DescriptorCollection(root.Elements().Select(ConvertToDescriptor));
    }

    private static CabInfoAttributeDescriptor ConvertToDescriptor(XElement arg)
    {
      var attributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(attributes["id"], 16);
      var name = attributes["name"];
      var converter = attributes["converter"];

      // Optional attributes
      string unit = attributes.ContainsKey("unit") ? attributes["unit"] : String.Empty;
      string comment = attributes.ContainsKey("comment") ? attributes["comment"] : String.Empty;

      return new CabInfoAttributeDescriptor(id, name, unit, converter, comment);
    }
  }
}