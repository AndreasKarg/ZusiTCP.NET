using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class DescriptorCollection : IEnumerable<AttributeDescriptor>, IEquatable<DescriptorCollection>
  {
    private readonly Dictionary<Address, AttributeDescriptor> _byId = new Dictionary<Address, AttributeDescriptor>();
    private readonly Dictionary<string, AttributeDescriptor> _byName = new Dictionary<string, AttributeDescriptor>();

    public DescriptorCollection(IEnumerable<AttributeDescriptor> descriptors)
    {
      var descriptorList = descriptors as IList<AttributeDescriptor> ?? descriptors.ToArray();
      foreach (var descriptor in descriptorList)
      {
        var name = descriptor.QualifiedName;
        var id = descriptor.Address;

        try
        {
          _byId.Add(id, descriptor);
        }
        catch (ArgumentException e)
        {
          var collidingNames = descriptorList.Where(d => d.Address == id)
                                             .Select(d => d.Name);
          var formattedArray = String.Join(", ", collidingNames);
          throw new InvalidDescriptorException(String.Format("Duplicate attribute descriptor with id 0x{0:x4} in source collection. Colliding names: {1}", id, formattedArray), e);
        }

        try
        {
          _byName.Add(name, descriptor);
        }
        catch (ArgumentException e)
        {
          var collidingIDs = descriptorList.Where(d => d.Name == name);
          var stringifiedIDs = collidingIDs.Select(d => String.Format("0x{0:x4}", d.Address));
          var formattedArray = String.Join(", ", stringifiedIDs);
          throw new InvalidDescriptorException(String.Format("Duplicate attribute descriptor with name '{0}' in source collection. Colliding IDs: {1}", name, formattedArray), e);
        }
      }
    }

    public AttributeDescriptor GetBy(string name)
    {
      return _byName[name];
    }

    public AttributeDescriptor GetBy(Address id)
    {
      return _byId[id];
    }

    public AttributeDescriptor this[string name]
    {
      get { return GetBy(name); }
    }

    public AttributeDescriptor this[Address id]
    {
      get { return GetBy(id); }
    }

    IEnumerator<AttributeDescriptor> IEnumerable<AttributeDescriptor>.GetEnumerator()
    {
      return _byId.Values.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable) _byId.Values).GetEnumerator();
    }

    #region Equality operations

    public bool Equals(DescriptorCollection other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      if (other.Count() != this.Count())
        return false;

      foreach (var descriptor in _byId)
      {
        if (!other._byId.Contains(descriptor))
          return false;
      }

      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((DescriptorCollection) obj);
    }

    public override int GetHashCode()
    {
      return _byId.Aggregate(0, ComputeHashCode);
    }

    private int ComputeHashCode(int aggregateHashCode, KeyValuePair<Address, AttributeDescriptor> descriptor)
    {
      var hashCode = (aggregateHashCode*397) ^ descriptor.Key.GetHashCode();
      return (hashCode * 397) ^ descriptor.Value.GetHashCode();
    }

    public static bool operator ==(DescriptorCollection left, DescriptorCollection right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(DescriptorCollection left, DescriptorCollection right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}