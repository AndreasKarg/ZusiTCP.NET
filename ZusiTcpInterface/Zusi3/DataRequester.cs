using System.Collections.Generic;
using System.Linq;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class DataRequester
  {
    private readonly DescriptorCollection<CabInfoNodeDescriptor> _descriptors;
    private readonly List<Address> _requestedAddresses = new List<Address>();

    public DataRequester(DescriptorCollection<CabInfoNodeDescriptor> descriptors)
    {
      _descriptors = descriptors;
    }

    public IReadOnlyCollection<Address> RequestedAddresses
    {
      get { return _requestedAddresses; }
    }

    public void Request(string attributeName)
    {
      var address = _descriptors.SelectMany(FindInNodeDescriptor);
    }

    private IEnumerable<Address> FindInNodeDescriptor(CabInfoNodeDescriptor descriptor)
    {
      throw new System.NotImplementedException();
    }
  }
}