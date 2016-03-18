using System;

namespace ZusiTcpInterface.Zusi3
{
  public struct CabInfoTypeDescriptor : IEquatable<CabInfoTypeDescriptor>
  {
    private readonly short _id;
    private readonly string _name;
    private readonly string _unit;
    private readonly string _type;
    private readonly string _comment;

    public CabInfoTypeDescriptor(short id, string name, string unit, string type, string comment = "")
    {
      _id = id;
      _name = name;
      _unit = unit;
      _type = type;
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

    public string Unit
    {
      get { return _unit; }
    }

    public string Type
    {
      get { return _type; }
    }

    public string Comment
    {
      get { return _comment; }
    }

    public bool Equals(CabInfoTypeDescriptor other)
    {
      return Id == other.Id
             && Name == other.Name
             && Unit == other.Unit
             && Type == other.Type
             && Comment == other.Comment;
    }
  }
}