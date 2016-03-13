using System;
using System.IO;

namespace ZusiTcpInterface.Zusi2
{
  internal class HandShaker
  {
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;

    public HandShaker(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
      _binaryReader = binaryReader;
      _binaryWriter = binaryWriter;
    }

    public void ShakeHands(ClientType clientType, String clientName)
    {
      var hello = new HelloPacket(clientType, clientName);

      hello.Serialise(_binaryWriter);
    }
  }
}