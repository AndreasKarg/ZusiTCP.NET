#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  internal class TCPServerSlaveConnection : Base_Connection
  {
    private HashSet<int> _requestedData;

    public TCPServerSlaveConnection(SynchronizationContext hostContext,
                                    IBinaryIO client,
                                    String clientId,
                                    ClientPriority priority)
      : base(clientId, priority, hostContext)
    {
      InitializeClient(client);
    }

    public HashSet<int> RequestedData
    {
      [DebuggerStepThrough]
      get
      {
        Debug.Assert(_requestedData != null);
        return _requestedData;
      }
    }

    public event EventHandler<EventArgs> DataRequested;

    private void OnDataRequested(ICollection<int> requestedData)
    {
      if (DataRequested == null) return;
      DataRequested.Invoke(this, EventArgs.Empty);
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
        if (_requestedData == null)
          _requestedData = new HashSet<int>(requestedValues.RequestedValues);
        else
          _requestedData.AddRange(requestedValues.RequestedValues);
        SendPacket(Pack(0, 4, 0));
      }

      OnDataRequested(_requestedData);
    }

    public void SendByteCommand(byte[] array, int id)
    {
      if ((ConnectionState != ConnectionState.Connected) || (!_requestedData.Contains(id)))
      {
        return;
      }
      var ida = new List<byte>(BitConverter.GetBytes(id));
      ida.RemoveAt(3);
      ida.Reverse();
      SendLargePacket(ida.ToArray(), array);
    }

    protected override void ReceiveLoop()
    {
      // TODO: Find out whether there can be something meaningful here and use it.

      ClientConnection.ReadByte();
      // The reader waits until a byte has been received without timeout.
      throw new NotSupportedException("A slave client sent data unexpectedly.");
    }
  }

  internal class DataRequestedEventArgs : EventArgs
  {
    private readonly ICollection<int> _requestedIds;

    public DataRequestedEventArgs(ICollection<int> requestedIds)
    {
      _requestedIds = requestedIds;
    }

    public ICollection<int> RequestedIds
    {
      get { return _requestedIds; }
    }
  }
}
