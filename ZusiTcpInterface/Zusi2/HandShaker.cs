using System;

namespace ZusiTcpInterface.Zusi2
{
  internal class HandShaker
  {
    private readonly IReadableStream _rxStream;
    private readonly IWritableStream _txStream;

    public HandShaker(IReadableStream rxStream, IWritableStream txStream)
    {
      _rxStream = rxStream;
      _txStream = txStream;
    }

    public void ShakeHands(ClientType clientType, String clientName)
    {
      var hello = new HelloPacket(clientType, clientName);

      _txStream.Write(hello.Serialise());
    }
  }
}