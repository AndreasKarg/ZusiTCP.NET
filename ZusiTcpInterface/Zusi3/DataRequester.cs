using System;
using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class DataRequester
  {
    private readonly DescriptorCollection<CabInfoNodeDescriptor> _descriptors;
    private readonly HashSet<Address> _requestedAddresses = new HashSet<Address>();

    public DataRequester(DescriptorCollection<CabInfoNodeDescriptor> descriptors)
    {
      _descriptors = descriptors;
    }

    public HashSet<Address> RequestedAddresses
    {
      get { return _requestedAddresses; }
    }

    public void Request(string attributeName)
    {
      var matchingAttribute = Find(attributeName);
      _requestedAddresses.Add(matchingAttribute.Address);
    }

    private CabInfoAttributeDescriptor Find(string attributeName)
    {
      var matchingAttribute = _descriptors.SelectMany(descriptor => FindInNodeDescriptor(descriptor, attributeName)).Single();
      return matchingAttribute;
    }

    private static IEnumerable<CabInfoAttributeDescriptor> FindInNodeDescriptor(CabInfoNodeDescriptor node, string attributeName)
    {
      var localAttributes = node.AttributeDescriptors.Where(attribute => attribute.Name == attributeName);

      var subNodeAttributes = node.NodeDescriptors.SelectMany(descriptor => FindInNodeDescriptor(descriptor, attributeName));

      return localAttributes.Concat(subNodeAttributes);
    }
  }
}