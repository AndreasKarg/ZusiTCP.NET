using System;
using System.Linq;

namespace ZusiTcpInterface.Zusi3
{
  public struct Address : IEquatable<Address>
  {
    private readonly int _hashValue;

    public Address(params short[] ids)
    {
      _hashValue = ids.Aggregate(0, AddToHash);
    }

    private static int AddToHash(int hashValue, short id)
    {
      hashValue *= 367;
      hashValue ^= id;

      return hashValue;
    }

    public bool Equals(Address other)
    {
      return _hashValue == other._hashValue;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((Address) obj);
    }

    public override int GetHashCode()
    {
      return _hashValue;
    }

    public static bool operator ==(Address left, Address right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Address left, Address right)
    {
      return !Equals(left, right);
    }
  }
}