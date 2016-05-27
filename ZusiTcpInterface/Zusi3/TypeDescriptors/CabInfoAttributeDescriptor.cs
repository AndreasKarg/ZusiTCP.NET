using System;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public class CabInfoAttributeDescriptor : CabInfoDescriptorBase, IEquatable<CabInfoAttributeDescriptor>
  {
    private readonly string _unit;
    private readonly string _type;

    public CabInfoAttributeDescriptor(short id, string name, string unit, string type, string comment = "") : base(id, name, comment)
    {
      if (unit == null) throw new ArgumentNullException("unit");
      if (type == null) throw new ArgumentNullException("type");

      _unit = unit;
      _type = type;
    }

    public string Unit
    {
      get { return _unit; }
    }

    public string Type
    {
      get { return _type; }
    }

    #region Equality operations

    public bool Equals(CabInfoAttributeDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return BaseEquals(other) && string.Equals(_unit, other._unit) && string.Equals(_type, other._type);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CabInfoAttributeDescriptor) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode*397) ^ _unit.GetHashCode();
        hashCode = (hashCode*397) ^ _type.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(CabInfoAttributeDescriptor left, CabInfoAttributeDescriptor right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(CabInfoAttributeDescriptor left, CabInfoAttributeDescriptor right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}