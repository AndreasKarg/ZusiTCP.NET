namespace ZusiTcpInterface.Enums.Lzb
{
  public enum StatusLzbUebertragungsausfall
  {
    Normal = 0,
    Eingeleitet = 1,
    LmUeBlinkt = 2,
    ErsteQuittierungErfolgt = 3,
    BedingungFuerZweiteQuittierungGegeben = 4,
    ZweiteQuittierungErfolgt = 5,
    AusfallNachVerdeckterLzbAufnahme = 6,
    AusfallNachVerdeckterLzbAufnahmeBefehlBlinkt = 7,
  }
}