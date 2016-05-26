using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public class CabInfoNodeDescriptor : CabInfoDescriptorBase, IEquatable<CabInfoNodeDescriptor>
  {
    private readonly ReadOnlyDictionary<short, CabInfoAttributeDescriptor> _attributeDescriptors;

    public CabInfoNodeDescriptor(short id, string name, string comment = "") : this(id, name, Enumerable.Empty<CabInfoAttributeDescriptor>(), comment)
    {
    }

    public CabInfoNodeDescriptor(short id, string name, IEnumerable<CabInfoAttributeDescriptor> attributeDescriptors, string comment = "")
      : base(id, name, comment)
    {
      var attributeDescriptors1 = attributeDescriptors.ToDictionary(descriptor => descriptor.Id);
      _attributeDescriptors = new ReadOnlyDictionary<short, CabInfoAttributeDescriptor>(attributeDescriptors1);
    }

    public ReadOnlyDictionary<short, CabInfoAttributeDescriptor> AttributeDescriptors
    {
      get { return _attributeDescriptors; }
    }

#region Equality operations

    public bool Equals(CabInfoNodeDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && _attributeDescriptors.Equals(other._attributeDescriptors);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CabInfoNodeDescriptor) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (base.GetHashCode()*397) ^ _attributeDescriptors.GetHashCode();
      }
    }

    public static bool operator ==(CabInfoNodeDescriptor left, CabInfoNodeDescriptor right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(CabInfoNodeDescriptor left, CabInfoNodeDescriptor right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}