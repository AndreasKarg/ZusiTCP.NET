using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  internal static class CabInfoTypeDescriptorReader
  {
    public static CabInfoNodeDescriptor ReadCommandsetFrom(Stream inputStream)
    {
      var root = XElement.Load(inputStream);

      return new CabInfoNodeDescriptor(0, "Root", root.Elements(XName.Get("Attribute")).Select(ConvertAttribute),
                                                  root.Elements(XName.Get("Node")).Select(ConvertNode));
    }

    private static CabInfoNodeDescriptor ConvertNode(XElement arg)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      // Optional attribute
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      return new CabInfoNodeDescriptor(id, name, comment);
    }

    private static CabInfoAttributeDescriptor ConvertAttribute(XElement arg)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      var converter = xmlAttributes["converter"];

      // Optional attributes
      string unit = xmlAttributes.ContainsKey("unit") ? xmlAttributes["unit"] : String.Empty;
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      return new CabInfoAttributeDescriptor(id, name, unit, converter, comment);
    }
  }
}