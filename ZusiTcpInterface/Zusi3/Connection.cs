using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3
{
  public class Connection : IDisposable
  {
    private readonly BlockingCollectionWrapper<IProtocolChunk> _receivedChunks = new BlockingCollectionWrapper<IProtocolChunk>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly TcpClient _tcpClient;
    private readonly Task _dataForwardingTask;
    private readonly Task _messageReceptionTask;
    private readonly IBlockingCollection<DataChunkBase> _receivedDataChunks = new BlockingCollectionWrapper<DataChunkBase>();
    private bool _hasBeenDisposed;

    internal Connection(string clientName, string clientVersion, IEnumerable<CabInfoAddress> neededData, IPEndPoint endPoint, RootNodeConverter rootNodeConverter)
    {
      //_tcpClient = new TcpClient(endPoint);
      //var endPoint = new IPEndPoint(new IPAddress(), );

      _tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
      var socket = _tcpClient.Client;
      socket.DualMode = true;
      socket.Connect(endPoint);

      var networkStream = new CancellableBlockingStream(_tcpClient.GetStream(), _cancellationTokenSource.Token);
      var binaryReader = new BinaryReader(networkStream);
      var binaryWriter = new BinaryWriter(networkStream);

      var messageReader = new MessageReceiver(binaryReader, rootNodeConverter, _receivedChunks);
      _messageReceptionTask = Task.Run(() => MessageReceptionLoop(messageReader));

      var handshaker = new Handshaker(_receivedChunks, binaryWriter, ClientType.ControlDesk, clientName, clientVersion,
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
          const int noTimeout = -1;
          _receivedChunks.TryTake(out protocolChunk, noTimeout, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
          // Teardown requested
          return;
        }
        _receivedDataChunks.Add((DataChunkBase) protocolChunk);
      }
    }

    private static void MessageReceptionLoop(MessageReceiver messageReader)
    {
      while (true)
      {
        try
        {
          messageReader.ProcessNextPacket();
        }
        catch (OperationCanceledException)
        {
          // Teardown requested
          return;
        }
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

      if (_messageReceptionTask != null && !_messageReceptionTask.Wait(500))
        throw new TimeoutException("Failed to shut down message recption task within timeout.");

      if (_dataForwardingTask != null && !_dataForwardingTask.Wait(500))
        throw new TimeoutException("Failed to shut down message forwarding task within timeout.");

      if (_tcpClient != null)
        _tcpClient.Close();

      _receivedDataChunks.CompleteAdding();
      _receivedChunks.CompleteAdding();

      _hasBeenDisposed = true;
    }
  }
}