using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class AddressBasedDescriptorCollection<T> : IEnumerable<T>, IEquatable<AddressBasedDescriptorCollection<T>> where T: AddressBasedDescriptorBase
  {
    private readonly Dictionary<Address, T> _byId = new Dictionary<Address, T>();
    private readonly Dictionary<string, T> _byName = new Dictionary<string, T>();

    public AddressBasedDescriptorCollection(IEnumerable<T> descriptors)
    {
      var descriptorList = descriptors as IList<T> ?? descriptors.ToArray();
      foreach (var descriptor in descriptorList)
      {
        var name = descriptor.Name;
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
          throw new InvalidDescriptorException(String.Format("Duplicate {0} with id 0x{1:x4} in source collection. Colliding names: {2}", typeof(T).Name, id, formattedArray), e);
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
          throw new InvalidDescriptorException(String.Format("Duplicate {0} with name '{1}' in source collection. Colliding IDs: {2}", typeof(T).Name, name, formattedArray), e);
        }
      }
    }

    public T GetBy(string name)
    {
      return _byName[name];
    }

    public T GetBy(Address id)
    {
      return _byId[id];
    }

    public T this[string name]
    {
      get { return GetBy(name); }
    }

    public T this[Address id]
    {
      get { return GetBy(id); }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _byId.Values.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable) _byId.Values).GetEnumerator();
    }

    #region Equality operations

    public bool Equals(AddressBasedDescriptorCollection<T> other)
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
      return Equals((AddressBasedDescriptorCollection<T>) obj);
    }

    public override int GetHashCode()
    {
      return _byId.Aggregate(0, ComputeHashCode);
    }

    private int ComputeHashCode(int aggregateHashCode, KeyValuePair<Address, T> descriptor)
    {
      var hashCode = (aggregateHashCode*397) ^ descriptor.Key.GetHashCode();
      return (hashCode * 397) ^ descriptor.Value.GetHashCode();
    }

    public static bool operator ==(AddressBasedDescriptorCollection<T> left, AddressBasedDescriptorCollection<T> right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(AddressBasedDescriptorCollection<T> left, AddressBasedDescriptorCollection<T> right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}
