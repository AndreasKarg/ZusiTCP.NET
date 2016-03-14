using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace ZusiTcpInterface.Zusi2
{
  public struct HelloPacket
  {
    private static readonly byte[] HelloInstruction = {0x00, 0x01};
    private const byte ProtocolVersion = 1;

    private readonly string _clientName;
    private readonly ClientType _clientType;

    public HelloPacket(ClientType clientType, string clientName) : this()
    {
      _clientName = clientName;
      _clientType = clientType;
    }

    [Pure]
    public ClientType ClientType
    {
      get { return _clientType; }
    }

    [Pure]
    public String ClientName
    {
      get { return _clientName; }
    }

    public void Serialise(BinaryWriter binaryWriter)
    {
      var payload = new List<byte>();
      payload.AddRange(HelloInstruction);
      payload.Add(ProtocolVersion);
      payload.Add((byte) ClientType);
      payload.Add((byte)_clientName.Length);
      payload.AddRange(Encoding.ASCII.GetBytes(_clientName));

      binaryWriter.Write(payload.Count);
      binaryWriter.Write(payload.ToArray());
    }
  }
}
