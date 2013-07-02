using System;
using System.Net;

namespace Zusi_Datenausgabe
{
  public interface IZusiTcpConnectionFactory : IDisposable
  {
    void Destroy();

    IZusiTcpClientConnection Create(string clientId, ClientPriority priority);

    IZusiTcpClientConnection Create(string clientId, ClientPriority priority, String commandsetPath);

    IZusiTcpClientConnection Create(string clientId, ClientPriority priority, XmlTcpCommands commands);
  }

  public interface INetworkIOHandlerFactory : IDisposable
  {
    void Close(INetworkIOHandler handler);

    INetworkIOHandler Create(IPEndPoint endPoint);
  }
}