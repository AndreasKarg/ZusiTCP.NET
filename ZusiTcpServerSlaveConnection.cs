#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Zusi_Datenausgabe
{
  internal class ZusiTcpServerSlaveConnection : ZusiTcpBaseConnection
  {
    private HashSet<int> _requestedData;
    private IBinaryIO _InitializerClient;

    public ZusiTcpServerSlaveConnection(SynchronizationContext hostContext,
                                    IBinaryIO client,
                                    String clientId,
                                    ClientPriority priority)
      : base(clientId, priority, hostContext)
    {
      _InitializerClient = client;
      //InitializeClient(client); //WARNING: Slave will no longer be Initialized automaticly. Server has to do it manually.
    }

    public void InitializeClient()
    {
      InitializeClient(_InitializerClient);
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
    ///<summary>Allows the Server to check if the Data Request can be terminated early. 
    /// If someone throws an exception in this Event, data will be refused.</summary>
    public event EventHandler<EventArgs> DataChecking;

    private void OnDataRequested(ICollection<int> requestedData)
    {
      if (DataRequested == null) return;
      DataRequested.Invoke(this, EventArgs.Empty);
    }
    private void OnDataChecking(ICollection<int> requestedData)
    {
      if (DataChecking == null) return;
      DataChecking.Invoke(this, EventArgs.Empty);
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

        try
        {
          HashSet<int> reqDatOld = _requestedData;
          _requestedData = new HashSet<int>(requestedValues.RequestedValues);
          OnDataChecking(requestedValues.RequestedValues);
          _requestedData = reqDatOld;
        }
        catch(System.Collections.Generic.KeyNotFoundException)
        {
          SendPacket(Pack(0, 4, 2));
          throw;
        }
        catch(NotSupportedException)
        {
          SendPacket(Pack(0, 4, 3));
          throw;
        }
        catch
        {
          SendPacket(Pack(0, 4, 0xFE));
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
