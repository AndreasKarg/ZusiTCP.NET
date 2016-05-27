using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class DescriptorCollection<T> : IEnumerable<T>, IEquatable<DescriptorCollection<T>> where T: CabInfoDescriptorBase
  {
    private readonly Dictionary<short, T> _byId;
    private readonly Dictionary<string, T> _byName;

    public DescriptorCollection(IEnumerable<T> descriptors)
    {
      var enumerable = descriptors as List<T> ?? descriptors.ToList();
      _byId = enumerable.ToDictionary(descriptor => descriptor.Id);
      _byName = enumerable.ToDictionary(descriptor => descriptor.Name);
    }

    public T GetBy(string name)
    {
      return _byName[name];
    }

    public T GetBy(short id)
    {
      return _byId[id];
    }

    public T this[string name]
    {
      get { return GetBy(name); }
    }

    public T this[short id]
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

    public bool Equals(DescriptorCollection<T> other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

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
      return Equals((DescriptorCollection<T>) obj);
    }

    public override int GetHashCode()
    {
      return _byId.Aggregate(0, ComputeHashCode);
    }

    private int ComputeHashCode(int aggregateHashCode, KeyValuePair<short, T> descriptor)
    {
      var hashCode = (aggregateHashCode*397) ^ descriptor.Key;
      return (hashCode * 397) ^ descriptor.Value.GetHashCode();
    }

    public static bool operator ==(DescriptorCollection<T> left, DescriptorCollection<T> right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(DescriptorCollection<T> left, DescriptorCollection<T> right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}