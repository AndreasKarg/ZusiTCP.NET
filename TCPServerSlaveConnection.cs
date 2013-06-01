using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Zusi_Datenausgabe
{
  internal class TCPServerSlaveConnection : Base_Connection
  {
    public event EventHandler DataRequested;

    private void OnDataRequested()
    {
      var handler = DataRequested;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    public TCPServerSlaveConnection(SynchronizationContext hostContext, TcpClient client, String clientId, ClientPriority priority)
      : base(clientId, priority, (TCPCommands)null, hostContext)
    {
      InitializeClient(client);
    }

    protected override void HandleHandshake()
    {
      SendPacket(Pack(0, 2, 0));
      ExpectResponseAnswer requestedValues = null;
      const int dataGroup = -1;
      while ((requestedValues == null) || (requestedValues.RequestedDataGroup != 0) ||
             ((requestedValues.RequestedValues != null) && (requestedValues.RequestedValues.Length != 0)))
      {
        try
        {
          requestedValues = ExpectResponse(ResponseType.NeededData, dataGroup);
        }
        catch
        {
          SendPacket(Pack(0, 4, 255));
          throw;
        }
        
        foreach (int requestedValue in requestedValues.RequestedValues)
        {
          if (!RequestedData.Contains(requestedValue))
            RequestedData.Add(requestedValue);
        }
        SendPacket(Pack(0, 4, 0));
      }

      OnDataRequested();
    }

    public void SendByteCommand(byte[] array, int id)
    {
      if ((ConnectionState != ConnectionState.Connected) || (!RequestedData.Contains(id)))
        return;
      List<byte> ida = new List<byte>(BitConverter.GetBytes(id));
      ida.RemoveAt(3);
      ida.Reverse();
      SendLargePacket(ida.ToArray(), array);
    }

    protected override void ReceiveLoop()
    {
      // TODO: Find out whether there can be something meaningful here and use it.

      ClientReader.ReadByte();
      // The reader waits until a byte has been received without timeout.
      throw new NotSupportedException("A slave client sent data unexpectedly.");
    }
  }
}