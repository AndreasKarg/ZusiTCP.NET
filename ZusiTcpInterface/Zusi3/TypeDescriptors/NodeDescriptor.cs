using System;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  [Obsolete]
  public class NodeDescriptor : DescriptorBase, IEquatable<NodeDescriptor>
  {
    private readonly DescriptorCollection<AttributeDescriptor> _attributeDescriptors;
    private readonly DescriptorCollection<NodeDescriptor> _nodeDescriptors;

    public NodeDescriptor(short address, string name, string comment = "")
      : this(address, name, Enumerable.Empty<AttributeDescriptor>(), comment)
    {
    }

    public NodeDescriptor(short address, string name, IEnumerable<AttributeDescriptor> attributeDescriptors, string comment = "")
      : this(address, name, attributeDescriptors, Enumerable.Empty<NodeDescriptor>(), comment)
    {
    }

    public NodeDescriptor(short address, string name, IEnumerable<AttributeDescriptor> attributeDescriptors, IEnumerable<NodeDescriptor> nodeDescriptors, string comment = "")
      : base(address, name, comment)
    {
      if (attributeDescriptors == null) throw new ArgumentNullException("attributeDescriptors");
      if (nodeDescriptors == null) throw new ArgumentNullException("nodeDescriptors");

      try
      {
        _attributeDescriptors = new DescriptorCollection<AttributeDescriptor>(attributeDescriptors);
      }
      catch (InvalidDescriptorException e)
      {
        throw new InvalidDescriptorException(String.Format("Error while processing attributes for node 0x{1:x4} - '{0}'", name, address), e);
      }

      try
      {
        _nodeDescriptors = new DescriptorCollection<NodeDescriptor>(nodeDescriptors);
      }
      catch (InvalidDescriptorException e)
      {
        throw new InvalidDescriptorException(String.Format("Error while processing child nodes of node 0x{1:x4} - '{0}'", name, address), e);
      }
    }

    public DescriptorCollection<AttributeDescriptor> AttributeDescriptors
    {
      get { return _attributeDescriptors; }
    }

    public DescriptorCollection<NodeDescriptor> NodeDescriptors
    {
      get { return _nodeDescriptors; }
    }

    #region Equality operations

    public bool Equals(NodeDescriptor other)
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
      return Equals((NodeDescriptor) obj);
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

    public static bool operator ==(NodeDescriptor left, NodeDescriptor right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(NodeDescriptor left, NodeDescriptor right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}