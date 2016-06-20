using System;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public abstract class AddressBasedDescriptorBase
  {
    private readonly CabInfoAddress _address;
    private readonly string _name;
    private readonly string _comment;

    protected AddressBasedDescriptorBase(CabInfoAddress address, string name, string comment)
    {
      if (name == null) throw new ArgumentNullException("name");
      if (comment == null) throw new ArgumentNullException("comment");

      _address = address;
      _name = name;
      _comment = comment;
    }

    public CabInfoAddress Address
    {
      get { return _address; }
    }

    public string Name
    {
      get { return _name; }
    }

    public string Comment
    {
      get { return _comment; }
    }

    protected bool BaseEquals(AddressBasedDescriptorBase other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return _address == other._address && string.Equals(_name, other._name) && string.Equals(_comment, other._comment);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = _address.GetHashCode();
        hashCode = (hashCode * 397) ^ _name.GetHashCode();
        hashCode = (hashCode * 397) ^ _comment.GetHashCode();
        return hashCode;
      }
    }
  }
}
