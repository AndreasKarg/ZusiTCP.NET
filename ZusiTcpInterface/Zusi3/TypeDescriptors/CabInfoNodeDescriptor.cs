using System;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public class CabInfoNodeDescriptor : CabInfoDescriptorBase, IEquatable<CabInfoNodeDescriptor>
  {
    private readonly DescriptorCollection<CabInfoAttributeDescriptor> _attributeDescriptors;
    private readonly DescriptorCollection<CabInfoNodeDescriptor> _nodeDescriptors;

    public CabInfoNodeDescriptor(short id, string name, string comment = "")
      : this(id, name, Enumerable.Empty<CabInfoAttributeDescriptor>(), comment)
    {
    }

    public CabInfoNodeDescriptor(short id, string name, IEnumerable<CabInfoAttributeDescriptor> attributeDescriptors, string comment = "")
      : this(id, name, attributeDescriptors, Enumerable.Empty<CabInfoNodeDescriptor>(), comment)
    {
    }

    public CabInfoNodeDescriptor(short id, string name, IEnumerable<CabInfoAttributeDescriptor> attributeDescriptors, IEnumerable<CabInfoNodeDescriptor> nodeDescriptors, string comment = "")
      : base(id, name, comment)
    {
      if (attributeDescriptors == null) throw new ArgumentNullException("attributeDescriptors");
      if (nodeDescriptors == null) throw new ArgumentNullException("nodeDescriptors");

      _attributeDescriptors = new DescriptorCollection<CabInfoAttributeDescriptor>(attributeDescriptors);
      _nodeDescriptors = new DescriptorCollection<CabInfoNodeDescriptor>(nodeDescriptors);
    }

    public DescriptorCollection<CabInfoAttributeDescriptor> AttributeDescriptors
    {
      get { return _attributeDescriptors; }
    }

    public DescriptorCollection<CabInfoNodeDescriptor> NodeDescriptors
    {
      get { return _nodeDescriptors; }
    }

    #region Equality operations

    public bool Equals(CabInfoNodeDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return _attributeDescriptors.Equals(other._attributeDescriptors) && _nodeDescriptors.Equals(other._nodeDescriptors);
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
        int hashCode = base.GetHashCode();
        hashCode = (hashCode*397) ^ _attributeDescriptors.GetHashCode();
        hashCode = (hashCode*397) ^ _nodeDescriptors.GetHashCode();
        return hashCode;
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

    public CabInfoAttributeDescriptor FindDescriptor(Address id)
    {
      throw new NotImplementedException();
    }
  }
}