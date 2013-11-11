using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zusi_Datenausgabe.AuxiliaryClasses;

namespace Zusi_Datenausgabe.NetworkIO
{
  public class HandshakeHandler
  {
    private readonly ASCIIEncoding _stringEncoder = new ASCIIEncoding();
    private readonly INetworkIOHandler _networkIOHandler;

    public HandshakeHandler(INetworkIOHandler networkIOHandler)
    {
      _networkIOHandler = networkIOHandler;
    }

    public void HandleHandshake(IEnumerable<int> requestedData, ClientPriority clientPriority, string clientId)
    {
      SendHello(clientPriority, clientId);
      ExpectAckHello();
      RequestData(requestedData);
      ExpectAckNeededData(0);
    }

    private void SendHello(ClientPriority clientPriority, string clientId)
    {
      _networkIOHandler.SendPacket(BitbangingHelpers.Pack(0, 1, 2, (byte) clientPriority, Convert.ToByte(_stringEncoder.GetByteCount(clientId))),
        _stringEncoder.GetBytes(clientId));
    }

    private void RequestData(IEnumerable<int> requestedData)
    {
      var aGetData = from iData in requestedData group iData by (iData/256);

      var reqDataBuffer = new List<byte[]>();

      foreach (var aDataGroup in aGetData)
      {
        reqDataBuffer.Clear();
        reqDataBuffer.Add(BitbangingHelpers.Pack(0, 3));

        byte[] tempDataGroup = BitConverter.GetBytes(Convert.ToInt16(aDataGroup.Key));
        reqDataBuffer.Add(BitbangingHelpers.Pack(tempDataGroup[1], tempDataGroup[0]));

        reqDataBuffer.AddRange(aDataGroup.Select(iID => BitbangingHelpers.Pack(Convert.ToByte(iID%256))));

        _networkIOHandler.SendPacket(reqDataBuffer.ToArray());

        ExpectAckNeededData(aDataGroup.Key);
      }

      SendDataRequestConclusion();
    }

    private void SendDataRequestConclusion()
    {
      _networkIOHandler.SendPacket(0, 3, 0, 0);
    }

    private void ExpectAckNeededData(int dataGroup)
    {
      var response = ReceiveResponse(ResponseType.AckNeededData);

      switch (response)
      {
        case 0:
          /* Response is an ACK. */
          return;
        case 1:
          throw new ZusiTcpException("Unknown instruction set: " + dataGroup);
        case 2:
          throw new ZusiTcpException("Client not connected");
        default:
          throw new ZusiTcpException("NEEDED_DATA not acknowledged.");
      }
    }

    private void ExpectAckHello()
    {
      var response = ReceiveResponse(ResponseType.AckHello);

      switch (response)
      {
        case 0:
          /* Response is an ACK. */
          return;
        case 1:
          throw new ZusiTcpException("Too many connections.");
        case 2:
          throw new ZusiTcpException("Zusi is already connected. No more connections allowed.");
        default:
          throw new ZusiTcpException("HELLO not acknowledged.");
      }
    }

    private int ReceiveResponse(ResponseType expectedInstruction)
    {
      int packetLength = _networkIOHandler.ReadInt32();
      if (packetLength != 3)
      {
        throw new ZusiTcpException("Invalid packet length: " + packetLength);
      }

      int readInstr = BitbangingHelpers.GetInstruction(_networkIOHandler.ReadByte(), _networkIOHandler.ReadByte());
      if (readInstr != (int) expectedInstruction)
      {
        throw new ZusiTcpException("Invalid command from server: " + readInstr);
      }

      int response = _networkIOHandler.ReadByte();
      return response;
    }

    #region Nested type: ResponseType
    private enum ResponseType
    {
      None = 0,

      AckHello = 2,

      AckNeededData = 4
    }
    #endregion
  }
}