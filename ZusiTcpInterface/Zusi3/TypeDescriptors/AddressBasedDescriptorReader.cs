using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  internal static class AddressBasedDescriptorReader
  {
    private static readonly string _namespace = "ZusiTcpInterface/CabInfoTypes";

    public static IEnumerable<AddressBasedAttributeDescriptor> ReadCommandsetFrom(Stream inputStream)
    {
      var root = XElement.Load(inputStream);

      var descriptors = ConvertRootNode(root);

      return descriptors;
    }

    private static IEnumerable<AddressBasedAttributeDescriptor> ConvertRootNode(XElement arg)
    {
      var baseAddress = new CabInfoAddress();
      var attributes = arg.Elements(XName.Get("Attribute", _namespace)).Select(xmlAttribute => ConvertAttribute(xmlAttribute, baseAddress, null));
      var attributesFromChildNodes = arg.Elements(XName.Get("Node", _namespace)).SelectMany(xmlNode => ConvertNode(xmlNode, baseAddress, null));

      return attributes.Concat(attributesFromChildNodes);
    }

    private static IEnumerable<AddressBasedAttributeDescriptor> ConvertNode(XElement arg, CabInfoAddress baseAddress, string baseName)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      // Optional attribute
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      var localAddress = baseAddress.Concat(id);

      var attributes = arg.Elements(XName.Get("Attribute", _namespace)).Select(xmlAttribute => ConvertAttribute(xmlAttribute, localAddress, ConcatenateNames(baseName, name)));
      var attributesFromChildNodes = arg.Elements(XName.Get("Node", _namespace)).SelectMany(xmlNode => ConvertNode(xmlNode, localAddress, ConcatenateNames(baseName, name)));

      return attributes.Concat(attributesFromChildNodes);
    }

    private static AddressBasedAttributeDescriptor ConvertAttribute(XElement arg, CabInfoAddress baseAddress, string baseName)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      var converter = xmlAttributes["converter"];

      // Optional attributes
      string unit = xmlAttributes.ContainsKey("unit") ? xmlAttributes["unit"] : String.Empty;
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      var localAddress = baseAddress.Concat(id);
      return new AddressBasedAttributeDescriptor(localAddress, ConcatenateNames(baseName, name), name, unit, converter, comment);
    }

    private static string ConcatenateNames(string baseName, string name)
    {
      return (baseName != null) ? String.Join(":", baseName, name) : name;
    }
  }
}