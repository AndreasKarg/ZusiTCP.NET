using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class DescriptorCollection
  {
    private readonly Dictionary<short, CabInfoDescriptorBase> _byId;
    private readonly Dictionary<string, CabInfoDescriptorBase> _byName;

    public DescriptorCollection(IEnumerable<CabInfoAttributeDescriptor> descriptors)
    {
      _byId = descriptors.ToDictionary(descriptor => descriptor.Id);
      _byName = descriptors.ToDictionary(descriptor => descriptor.Name);
    }

    public CabInfoDescriptorBase GetBy(string name)
    {
      return _byName[name];
    }

    public CabInfoDescriptorBase GetBy(short id)
    {
      return _byId[id];
    }

    public CabInfoDescriptorBase this[string name]
    {
      get { return GetBy(name); }
    }

    public CabInfoDescriptorBase this[short id]
    {
      get { return GetBy(id); }
    }
  }
}