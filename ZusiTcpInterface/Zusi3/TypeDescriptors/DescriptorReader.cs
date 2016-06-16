using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  internal static class DescriptorReader
  {
    private static readonly string _namespace = "ZusiTcpInterface/CabInfoTypes";

    public static NodeDescriptor ReadCommandsetFrom(Stream inputStream)
    {
      var root = XElement.Load(inputStream);

      return new NodeDescriptor(0x0A, "Root", root.Elements(XName.Get("Attribute", _namespace)).Select(ConvertAttribute),
                                                  root.Elements(XName.Get("Node", _namespace)).Select(ConvertNode));
    }

    private static NodeDescriptor ConvertNode(XElement arg)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      // Optional attribute
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      return new NodeDescriptor(id, name, arg.Elements(XName.Get("Attribute", _namespace)).Select(ConvertAttribute),
                                                 arg.Elements(XName.Get("Node", _namespace)).Select(ConvertNode), comment);
    }

    private static AttributeDescriptor ConvertAttribute(XElement arg)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      var converter = xmlAttributes["converter"];

      // Optional attributes
      string unit = xmlAttributes.ContainsKey("unit") ? xmlAttributes["unit"] : String.Empty;
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      return new AttributeDescriptor(id, name, unit, converter, comment);
    }
  }
}