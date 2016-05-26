using System;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public abstract class CabInfoDescriptorBase
  {
    private readonly short _id;
    private readonly string _name;
    private readonly string _comment;

    protected CabInfoDescriptorBase(short id, string name, string comment)
    {
      if (name == null) throw new ArgumentNullException("name");
      if (comment == null) throw new ArgumentNullException("comment");

      _id = id;
      _name = name;
      _comment = comment;
    }

    public short Id
    {
      get { return _id; }
    }

    public string Name
    {
      get { return _name; }
    }

    public string Comment
    {
      get { return _comment; }
    }

    protected bool Equals(CabInfoDescriptorBase other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return _id == other._id && string.Equals(_name, other._name) && string.Equals(_comment, other._comment);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = _id.GetHashCode();
        hashCode = (hashCode * 397) ^ _name.GetHashCode();
        hashCode = (hashCode * 397) ^ _comment.GetHashCode();
        return hashCode;
      }
    }
  }
}