using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ZusiTcpInterface.Converters;
using ZusiTcpInterface.DOM;

namespace ZusiTcpInterface
{
  public class Connection : IDisposable
  {
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly TcpClient _tcpClient;
    private readonly Task _dataForwardingTask;
    private readonly IBlockingCollection<DataChunkBase> _receivedDataChunks = new BlockingCollectionWrapper<DataChunkBase>();
    private bool _hasBeenDisposed;
    private readonly MessageReceiver _messageReceiver;

    internal Connection(string clientName, string clientVersion, IEnumerable<CabInfoAddress> neededData, RootNodeConverter rootNodeConverter,
      IPEndPoint endPoint)
    {
      _tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
      var socket = _tcpClient.Client;
      socket.DualMode = true;
      socket.Connect(endPoint);
    }

    internal Connection(string clientName, string clientVersion, IEnumerable<CabInfoAddress> neededData, RootNodeConverter rootNodeConverter)
    {
      var cancellableStream = new CancellableBlockingStream(_tcpClient.GetStream(), _cancellationTokenSource.Token);
      var binaryReader = new BinaryReader(cancellableStream);
      var binaryWriter = new BinaryWriter(cancellableStream);

      _messageReceiver = new MessageReceiver(binaryReader, rootNodeConverter);

      var handshaker = new Handshaker(_messageReceiver, binaryWriter, ClientType.ControlDesk, clientName, clientVersion,
        neededData);

      handshaker.ShakeHands();

      _dataForwardingTask = Task.Run((Action) DataForwardingLoop);
    }

    private void DataForwardingLoop()
    {
      while (true)
      {
        IProtocolChunk protocolChunk;
        try
        {
          protocolChunk = _messageReceiver.GetNextChunk();
        }
        catch (OperationCanceledException)
        {
          // Teardown requested
          return;
        }
        _receivedDataChunks.Add((DataChunkBase) protocolChunk);
      }
    }

    public IBlockingCollection<DataChunkBase> ReceivedDataChunks
    {
      get { return _receivedDataChunks; }
    }

    public void Dispose()
    {
      if (_hasBeenDisposed)
        return;

      _cancellationTokenSource.Cancel();

      if (_dataForwardingTask != null && !_dataForwardingTask.Wait(500))
        throw new TimeoutException("Failed to shut down message forwarding task within timeout.");

      if (_tcpClient != null)
        _tcpClient.Close();

      _receivedDataChunks.CompleteAdding();

      _hasBeenDisposed = true;
    }
  }
}