using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Zusi_Datenausgabe
{
  internal class TCPServerSlaveConnection : Base_Connection
  {
    private HashSet<int> _requestedData;
    public event EventHandler<EventArgs> DataRequested;

    public HashSet<int> RequestedData
    {
      [DebuggerStepThrough]
      get
      {
        Debug.Assert(_requestedData != null);
        return _requestedData;
      }
    }

    private void OnDataRequested(ICollection<int> requestedData)
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
        
        // TODO: Check for correct data group
        _requestedData = new HashSet<int>(requestedValues.RequestedValues);
        SendPacket(Pack(0, 4, 0));
      }

      OnDataRequested(_requestedData);
    }

    public void SendByteCommand(byte[] array, int id)
    {
      if ((ConnectionState != ConnectionState.Connected) || (!_requestedData.Contains(id)))
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

  internal class DataRequestedEventArgs : EventArgs
  {
    public DataRequestedEventArgs(ICollection<int> requestedIds)
    {
      _requestedIds = requestedIds;
    }

    private readonly ICollection<int> _requestedIds;

    public ICollection<int> RequestedIds
    {
      get { return _requestedIds; }
    }
  }
}