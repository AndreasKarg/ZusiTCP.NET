namespace Zusi_Datenausgabe
{
  // TODO: Maybe get rid of this?
  public interface ICommandEntry
  {
    [System.Xml.Serialization.XmlAttributeAttribute()]
    int ID { get; set; }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    string Name { get; set; }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    string Type { get; set; }
  }
}