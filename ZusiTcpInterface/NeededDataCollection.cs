using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.TypeDescriptors;

namespace ZusiTcpInterface
{
  public class NeededDataCollection
  {
    private readonly DescriptorCollection _descriptorCollection;
    private readonly HashSet<AttributeDescriptor> _requestedDescriptors = new HashSet<AttributeDescriptor>();

    public NeededDataCollection(DescriptorCollection descriptorCollection)
    {
      _descriptorCollection = descriptorCollection;
    }

    public void Request(params string[] namesOfAttributes)
    {
      foreach (string attributeName in namesOfAttributes)
      {
        _requestedDescriptors.Add(_descriptorCollection.GetBy(attributeName));
      }
    }

    public void Request(params CabInfoAddress[] addressesOfAttributes)
    {
      foreach (var address in addressesOfAttributes)
      {
        _requestedDescriptors.Add(_descriptorCollection.GetBy(address));
      }
    }

    public IEnumerable<AttributeDescriptor> GetRequestedDescriptors()
    {
      return _requestedDescriptors;
    }

    public IEnumerable<CabInfoAddress> GetRequestedAddresses()
    {
      return _requestedDescriptors.Select(descriptor => descriptor.Address);
    }
  }
}