using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class DescriptorCollection
  {
    private readonly Dictionary<short, CabInfoTypeDescriptor> _byId;
    private readonly Dictionary<string, CabInfoTypeDescriptor> _byName;

    public DescriptorCollection(IEnumerable<CabInfoTypeDescriptor> descriptors)
    {
      _byId = descriptors.ToDictionary(descriptor => descriptor.Id);
      _byName = descriptors.ToDictionary(descriptor => descriptor.Name);
    }

    public CabInfoTypeDescriptor GetBy(string name)
    {
      return _byName[name];
    }

    public CabInfoTypeDescriptor GetBy(short id)
    {
      return _byId[id];
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