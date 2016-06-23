using System;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public abstract class AddressBasedDescriptorBase
  {
    private readonly CabInfoAddress _address;
    private readonly string _qualifiedName;
    private readonly string _name;
    private readonly string _comment;

    protected AddressBasedDescriptorBase(CabInfoAddress address, string qualifiedName, string name, string comment)
    {
      if (qualifiedName == null)
        throw new ArgumentNullException("qualifiedName");
      if (name == null) throw new ArgumentNullException("name");
      if (comment == null) throw new ArgumentNullException("comment");

      _address = address;
      _qualifiedName = qualifiedName;
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

    public string QualifiedName
    {
      get { return _qualifiedName; }
    }

    protected bool BaseEquals(AddressBasedDescriptorBase other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return _address == other._address && string.Equals(_qualifiedName, other._qualifiedName) && string.Equals(_name, other._name) && string.Equals(_comment, other._comment);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = _address.GetHashCode();
        hashCode = (hashCode * 397) ^ _qualifiedName.GetHashCode();
        hashCode = (hashCode * 397) ^ _name.GetHashCode();
        hashCode = (hashCode * 397) ^ _comment.GetHashCode();
        return hashCode;
      }
    }
  }
}