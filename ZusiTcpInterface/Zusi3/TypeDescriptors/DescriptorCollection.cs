using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class DescriptorCollection
  {
    private readonly Dictionary<short, CabInfoAttributeDescriptor> _byId;
    private readonly Dictionary<string, CabInfoAttributeDescriptor> _byName;

    public DescriptorCollection(IEnumerable<CabInfoAttributeDescriptor> descriptors)
    {
      _byId = descriptors.ToDictionary(descriptor => descriptor.Id);
      _byName = descriptors.ToDictionary(descriptor => descriptor.Name);
    }

    public CabInfoAttributeDescriptor GetBy(string name)
    {
      return _byName[name];
    }

    public CabInfoAttributeDescriptor GetBy(short id)
    {
      return _byId[id];
    }

    public CabInfoAttributeDescriptor this[string name]
    {
      get { return GetBy(name); }
    }

    public CabInfoAttributeDescriptor this[short id]
    {
      get { return GetBy(id); }
    }
  }
}