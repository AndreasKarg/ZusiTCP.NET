using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  public class DescriptorCollection
  {
    private readonly List<CabInfoTypeDescriptor> _descriptors;

    public DescriptorCollection(List<CabInfoTypeDescriptor> descriptors)
    {
      _descriptors = descriptors;
    }

    public CabInfoTypeDescriptor GetBy(string name)
    {
      return _descriptors.Single(descriptor => descriptor.Name == name);
    }

    public CabInfoTypeDescriptor GetBy(short id)
    {
      return _descriptors.Single(descriptor => descriptor.Id == id);
    }

    public CabInfoTypeDescriptor this[string name]
    {
      get { return GetBy(name); }
    }

    public CabInfoTypeDescriptor this[short id]
    {
      get { return GetBy(id); }
    }
  }
}