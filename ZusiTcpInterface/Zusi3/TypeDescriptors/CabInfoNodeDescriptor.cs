using System;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public class CabInfoNodeDescriptor : CabInfoDescriptorBase, IEquatable<CabInfoNodeDescriptor>
  {
    private readonly DescriptorCollection<CabInfoAttributeDescriptor> _attributeDescriptors;
    private readonly DescriptorCollection<CabInfoNodeDescriptor> _nodeDescriptors;

    public CabInfoNodeDescriptor(Address address, string name, string comment = "")
      : this(address, name, Enumerable.Empty<CabInfoAttributeDescriptor>(), comment)
    {
    }

    public CabInfoNodeDescriptor(Address address, string name, IEnumerable<CabInfoAttributeDescriptor> attributeDescriptors, string comment = "")
      : this(address, name, attributeDescriptors, Enumerable.Empty<CabInfoNodeDescriptor>(), comment)
    {
    }

    public CabInfoNodeDescriptor(Address address, string name, IEnumerable<CabInfoAttributeDescriptor> attributeDescriptors, IEnumerable<CabInfoNodeDescriptor> nodeDescriptors, string comment = "")
      : base(address, name, comment)
    {
      if (attributeDescriptors == null) throw new ArgumentNullException("attributeDescriptors");
      if (nodeDescriptors == null) throw new ArgumentNullException("nodeDescriptors");

      try
      {
        _attributeDescriptors = new DescriptorCollection<CabInfoAttributeDescriptor>(attributeDescriptors);
      }
      catch (InvalidDescriptorException e)
      {
        throw new InvalidDescriptorException(String.Format("Error while processing attributes for node 0x{1:x4} - '{0}'", name, address), e);
      }

      try
      {
        _nodeDescriptors = new DescriptorCollection<CabInfoNodeDescriptor>(nodeDescriptors);
      }
      catch (InvalidDescriptorException e)
      {
        throw new InvalidDescriptorException(String.Format("Error while processing child nodes of node 0x{1:x4} - '{0}'", name, address), e);
      }
    }

    public CabInfoAttributeDescriptor FindDescriptor(Address address)
    {
      if (address.Ids.Count == 1)
        return _attributeDescriptors[address.Single()];

      return _nodeDescriptors[address.First()].FindDescriptor(new Address(address.Skip(1).ToArray()));
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
  }
}