using System;

namespace ZusiTcpInterface.Zusi3.TypeDescriptors
{
  public class CabInfoAttributeDescriptor : CabInfoDescriptorBase
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
  }
}