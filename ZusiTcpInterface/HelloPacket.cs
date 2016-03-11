using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using MiscUtil.Conversion;

namespace ZusiTcpInterface
{
  public struct HelloPacket
  {
    private static readonly byte[] HelloInstruction = {0x00, 0x01};
    private const byte ProtocolVersion = 1;
    private const Int32 HeaderLength = 5; // 2 for instruction, 1 for protocol version, 1 for client type, 1 for string length

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

    [Pure]
    public IEnumerable<byte> Serialise()
    {
      var bitConverter = EndianBitConverter.Little;
      Int32 expectedPacketLength = HeaderLength + _clientName.Length;

      var serialisedPacketLength = bitConverter.GetBytes(expectedPacketLength);

      var payload = new List<byte>(expectedPacketLength);
      payload.AddRange(HelloInstruction);
      payload.Add(ProtocolVersion);
      payload.Add((byte) ClientType);
      payload.Add((byte)_clientName.Length);
      payload.AddRange(Encoding.ASCII.GetBytes(_clientName));

      return serialisedPacketLength.Concat(payload);
    }
  }
}
