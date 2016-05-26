using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public class CabInfoNodeDescriptor : CabInfoDescriptorBase
  {
    private readonly ReadOnlyDictionary<short, CabInfoAttributeDescriptor> _attributeDescriptors;

    public CabInfoNodeDescriptor(short id, string name, string comment = "") : this(id, name, Enumerable.Empty<CabInfoAttributeDescriptor>(), comment)
    {
    }

    public CabInfoNodeDescriptor(short id, string name, IEnumerable<CabInfoAttributeDescriptor> attributeDescriptors, string comment = "")
      : base(id, name, comment)
    {
      if (attributeDescriptors == null) throw new ArgumentNullException("attributeDescriptors");

      var attributeDescriptors1 = attributeDescriptors.ToDictionary(descriptor => descriptor.Id);
      _attributeDescriptors = new ReadOnlyDictionary<short, CabInfoAttributeDescriptor>(attributeDescriptors1);
    }

    public ReadOnlyDictionary<short, CabInfoAttributeDescriptor> AttributeDescriptors
    {
      get { return _attributeDescriptors; }
    }
  }
}