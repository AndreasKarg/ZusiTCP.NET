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

    public static IEnumerable<AddressBasedAttributeDescriptor> ReadCommandsetFrom(Stream inputStream, Address baseAddress)
    {
      var root = XElement.Load(inputStream);

      var descriptors = ConvertRootNode(root, baseAddress);

      return descriptors;
    }

    private static IEnumerable<AddressBasedAttributeDescriptor> ConvertRootNode(XElement arg, Address baseAddress)
    {
      var attributes = arg.Elements(XName.Get("Attribute", _namespace)).Select(xmlAttribute => ConvertAttribute(xmlAttribute, baseAddress));
      var attributesFromChildNodes = arg.Elements(XName.Get("Node", _namespace)).SelectMany(xmlNode => ConvertNode(xmlNode, baseAddress));

      return attributes.Concat(attributesFromChildNodes);
    }

    private static IEnumerable<AddressBasedAttributeDescriptor> ConvertNode(XElement arg, Address baseAddress)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      // Optional attribute
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      var localAddress = baseAddress.Concat(id);

      var attributes = arg.Elements(XName.Get("Attribute", _namespace)).Select(xmlAttribute => ConvertAttribute(xmlAttribute, localAddress));
      var attributesFromChildNodes = arg.Elements(XName.Get("Node", _namespace)).SelectMany(xmlNode => ConvertNode(xmlNode, localAddress));

      return attributes.Concat(attributesFromChildNodes);
    }

    private static AddressBasedAttributeDescriptor ConvertAttribute(XElement arg, Address baseAddress)
    {
      var xmlAttributes = arg.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.InvariantCultureIgnoreCase);

      var id = Convert.ToInt16(xmlAttributes["id"], 16);
      var name = xmlAttributes["name"];

      var converter = xmlAttributes["converter"];

      // Optional attributes
      string unit = xmlAttributes.ContainsKey("unit") ? xmlAttributes["unit"] : String.Empty;
      string comment = xmlAttributes.ContainsKey("comment") ? xmlAttributes["comment"] : String.Empty;

      var localAddress = baseAddress.Concat(id);
      return new AddressBasedAttributeDescriptor(localAddress, name, unit, converter, comment);
    }
  }
}
