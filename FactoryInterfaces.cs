using System;

namespace Zusi_Datenausgabe
{
  public interface IZusiTcpConnectionFactory : IDisposable
  {
    void Destroy();

    IZusiTcpClientConnection Create(string clientId, ClientPriority priority);

    IZusiTcpClientConnection Create(string clientId, ClientPriority priority, String commandsetPath);

    IZusiTcpClientConnection Create(string clientId, ClientPriority priority, TcpCommands.XmlTcpCommands commands);
  }
}