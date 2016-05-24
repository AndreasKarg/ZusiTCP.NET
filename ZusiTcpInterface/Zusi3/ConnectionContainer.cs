﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ZusiTcpInterface.Zusi3
{
  public class ConnectionContainer : IDisposable
  {
    private DescriptorCollection _descriptors;
    private readonly HashSet<short> _neededData = new HashSet<short>();
    private static RootNodeConverter _rootNodeConverter;
    private readonly IBlockingCollection<CabDataChunkBase> _receivedCabDataChunks = new BlockingCollectionWrapper<CabDataChunkBase>();
    private readonly BlockingCollectionWrapper<IProtocolChunk> _receivedChunks = new BlockingCollectionWrapper<IProtocolChunk>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private string _clientName = "Unnamed";
    private string _clientVersion = "Unknown";

    #region Fields involved in object disposal

    private TcpClient _tcpClient;
    private Task _cabDataForwardingTask;
    private bool _hasBeenDisposed;
    private Task _messageReceptionTask;

    #endregion Fields involved in object disposal

    public DescriptorCollection Descriptors
    {
      get { return _descriptors; }
    }

    public HashSet<short> NeededData
    {
      get { return _neededData; }
    }

    public IBlockingCollection<CabDataChunkBase> ReceivedCabDataChunks
    {
      get { return _receivedCabDataChunks; }
    }

    public string ClientName
    {
      get { return _clientName; }
      set { _clientName = value; }
    }

    public string ClientVersion
    {
      get { return _clientVersion; }
      set { _clientVersion = value; }
    }

    public ConnectionContainer(string cabInfoTypeDescriptorFilename = "Zusi3/CabInfoTypes.csv")
    {
      using (var commandSetFileStream = File.OpenRead(cabInfoTypeDescriptorFilename))
      {
        InitialiseFrom(commandSetFileStream);
      }
    }

    public ConnectionContainer(Stream commandsetFileStream)
    {
      InitialiseFrom(commandsetFileStream);
    }

    public ConnectionContainer(IEnumerable<CabInfoTypeDescriptor> cabInfoTypeDescriptors)
    {
      InitialiseFrom(cabInfoTypeDescriptors.ToList());
    }

    private void InitialiseFrom(Stream fileStream)
    {
      var cabInfoDescriptors = CabInfoTypeDescriptorReader.ReadCommandsetFrom(fileStream).ToList();
      InitialiseFrom(cabInfoDescriptors);
    }

    private void InitialiseFrom(List<CabInfoTypeDescriptor> cabInfoTypeDescriptors)
    {
      _descriptors = new DescriptorCollection(cabInfoTypeDescriptors);
      var cabInfoConversionFunctions = GenerateConversionFunctions(cabInfoTypeDescriptors);

      SetupNodeConverters(cabInfoConversionFunctions);
    }

    private static void SetupNodeConverters(Dictionary<short, Func<short, byte[], IProtocolChunk>> cabInfoConversionFunctions)
    {
      var handshakeConverter = new NodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter.SubNodeConverters[0x02] = ackHelloConverter;

      var cabDataConverter = new NodeConverter{ ConversionFunctions = cabInfoConversionFunctions};
      var userDataConverter = new NodeConverter();
      userDataConverter.SubNodeConverters[0x04] = ackNeededDataConverter;
      userDataConverter.SubNodeConverters[0x0A] = cabDataConverter;

      _rootNodeConverter = new RootNodeConverter();
      _rootNodeConverter[0x01] = handshakeConverter;
      _rootNodeConverter[0x02] = userDataConverter;
    }

    private Dictionary<short, Func<short, byte[], IProtocolChunk>> GenerateConversionFunctions(IEnumerable<CabInfoTypeDescriptor> cabInfoDescriptors)
    {
      var descriptorToConversionFunctionMap = new Dictionary<string, Func<short, byte[], IProtocolChunk>>()
      {
        {"single", AttributeConverters.ConvertSingle},
        {"boolassingle", AttributeConverters.ConvertBoolAsSingle},
        {"fail", (s, bytes) => {throw new NotSupportedException("Unsupported data type received");} }
      };

      return cabInfoDescriptors.ToDictionary(descriptor => descriptor.Id,
                                             descriptor => descriptorToConversionFunctionMap[descriptor.Type.ToLowerInvariant()]);
    }

    public void RequestData(string name)
    {
      _neededData.Add(_descriptors.GetBy(name).Id);
    }

    public void RequestData(params CabInfoTypeDescriptor[] descriptors)
    {
      foreach (var descriptor in descriptors)
      {
        _neededData.Add(descriptor.Id);
      }
    }

    public void Dispose()
    {
      if (_hasBeenDisposed)
        return;

      _cancellationTokenSource.Cancel();

      if(_messageReceptionTask != null && !_messageReceptionTask.Wait(500))
        throw new TimeoutException("Failed to shut down message recption task within timeout.");
      _messageReceptionTask = null;

      if (_cabDataForwardingTask != null && !_cabDataForwardingTask.Wait(500))
        throw new TimeoutException("Failed to shut down message forwarding task within timeout.");
      _cabDataForwardingTask = null;

      if(_tcpClient != null)
        _tcpClient.Close();

      _receivedCabDataChunks.CompleteAdding();
      _receivedChunks.CompleteAdding();

      _hasBeenDisposed = true;
    }

    public void Connect(string hostname = "localhost", int port = 1436)
    {
      _tcpClient = new TcpClient(hostname, port);

      var networkStream = new CancellableBlockingStream(_tcpClient.GetStream(), _cancellationTokenSource.Token);
      var binaryReader = new BinaryReader(networkStream);
      var binaryWriter = new BinaryWriter(networkStream);

      var messageReader = new MessageReceiver(binaryReader, _rootNodeConverter, _receivedChunks);
      _messageReceptionTask = Task.Run(() =>
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
      });

      var handshaker = new Handshaker(_receivedChunks, binaryWriter, ClientType.ControlDesk, _clientName, _clientVersion,
        _neededData);

      handshaker.ShakeHands();

      _cabDataForwardingTask = Task.Run(() =>
      {
        while (true)
        {
          IProtocolChunk protocolChunk;
          try
          {
            _receivedChunks.TryTake(out protocolChunk, -1, _cancellationTokenSource.Token);
          }
          catch (OperationCanceledException)
          {
            // Teardown requested
            return;
          }
          _receivedCabDataChunks.Add((CabDataChunkBase) protocolChunk);
        }
      });
    }
  }
}