using System;

namespace Zusi_Datenausgabe
{
  [Obsolete("This class has been renamed and may be removed in v2.0. Use new name ZusiTcpClientConnection instead.")]
  public class ZusiTcpConn : ZusiTcpClientConnection
  {
    public ZusiTcpConn(string clientId, ClientPriority priority, string commandsetPath = "commandset.xml")
      : base(clientId, priority, commandsetPath)
    {
    }

    public ZusiTcpConn(string clientId, ClientPriority priority, ITcpCommandDictionary commands) : base(clientId, priority, commands)
    {
    }
  }

  [Obsolete("This type is obsolete. Use DataReceivedEventArgs<> instead.", true)]
  public struct ZusiData {}
}
