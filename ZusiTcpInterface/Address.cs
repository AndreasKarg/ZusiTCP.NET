using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ZusiTcpInterface
{
  public class Address : IEquatable<Address>, IEnumerable<short>
  {
    private readonly short[] _ids;
    private readonly int _hashValue;

    public Address(params short[] ids)
    {
      if (ids == null) throw new ArgumentNullException("ids");

      _ids = ids;
      _hashValue = ids.Aggregate(0, AddToHash);
    }

    [Pure]
    public Address Concat(short suffix)
    {
      return new Address(Ids.Concat(new[] {suffix}).ToArray());
    }

    public ReadOnlyCollection<short> Ids
    {
      get { return Array.AsReadOnly(_ids); }
    }

    private static int AddToHash(int hashValue, short id)
    {
      hashValue *= 367;
      hashValue ^= id;

      return hashValue;
    }

    public override string ToString()
    {
      var formattedIds = _ids.Select(id => String.Format("0x{0:x2}", id));

      return "{" + String.Join(", ", formattedIds) + "}";
    }

    #region Equality operations

    public bool Equals(Address other)
    {
      return _hashValue == other._hashValue;
    }

    public IEnumerator<short> GetEnumerator()
    {
      return _ids.AsEnumerable().GetEnumerator();
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (!(obj is Address)) return false;
      return Equals((Address) obj);
    }

    public override int GetHashCode()
    {
      return _hashValue;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _ids.GetEnumerator();
    }

    public static bool operator ==(Address left, Address right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Address left, Address right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}