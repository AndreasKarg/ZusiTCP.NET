using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Enums;
using ZusiTcpInterface.Zusi3.Enums.Lzb;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterface.Zusi3
{
  public class ConnectionContainer : IDisposable
  {
    private NodeDescriptor _descriptors;
    private readonly HashSet<short> _neededData = new HashSet<short>();
    private RootNodeConverter _rootNodeConverter;
    private readonly IBlockingCollection<DataChunkBase> _receivedDataChunks = new BlockingCollectionWrapper<DataChunkBase>();
    private readonly BlockingCollectionWrapper<IProtocolChunk> _receivedChunks = new BlockingCollectionWrapper<IProtocolChunk>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private string _clientName = "Unnamed";
    private string _clientVersion = "Unknown";

    private readonly Dictionary<string, Func<Address, byte[], IProtocolChunk>> _converterMap = new Dictionary<string, Func<Address, byte[], IProtocolChunk>>(StringComparer.InvariantCultureIgnoreCase)
      {
        {"single", AttributeConverters.ConvertSingle},
        {"boolassingle", AttributeConverters.ConvertBoolAsSingle},
        {"boolasbyte", AttributeConverters.ConvertBoolAsByte},
        {"string", AttributeConverters.ConvertString},
        {"zugart", AttributeConverters.ConvertEnumAsShort<Zugart>},
        {"switchstate", AttributeConverters.ConvertEnumAsByte<SwitchState>},
        {"aktivezugdaten", AttributeConverters.ConvertEnumAsShort<AktiveZugdaten>},
        {"statussifahupe", AttributeConverters.ConvertEnumAsByte<StatusSifaHupe>},
        {"zustandzugsicherung", AttributeConverters.ConvertEnumAsShort<ZustandZugsicherung>},
        {"grundzwangsbremsung", AttributeConverters.ConvertEnumAsShort<GrundZwangsbremsung>},
        {"lzbzustand", AttributeConverters.ConvertEnumAsShort<LzbZustand>},
        {"statuslzbuebertragungsausfall", AttributeConverters.ConvertEnumAsShort<StatusLzbUebertragungsausfall>},
        {"indusihupe", AttributeConverters.ConvertEnumAsByte<IndusiHupe>},
        {"zusatzinfomelderbild", AttributeConverters.ConvertEnumAsByte<ZusatzinfoMelderbild>},
        {"pilotlightstate", AttributeConverters.ConvertEnumAsByte<PilotLightState>},
        {"statusendeverfahren", AttributeConverters.ConvertEnumAsByte<StatusEndeVerfahren>},
        {"statusauftrag", AttributeConverters.ConvertEnumAsByte<StatusAuftrag>},
        {"statusvorsichtauftrag", AttributeConverters.ConvertEnumAsByte<StatusVorsichtauftrag>},
        {"statusnothalt", AttributeConverters.ConvertEnumAsByte<StatusLzbNothalt>},
        {"statusrechnerausfall", AttributeConverters.ConvertEnumAsByte<StatusRechnerausfall>},
        {"statuselauftrag", AttributeConverters.ConvertEnumAsByte<StatusElAuftrag>},
        {"short", AttributeConverters.ConvertShort},
        {"fail", (s, bytes) => {throw new NotSupportedException("Unsupported data type received");} }
      };

    #region Fields involved in object disposal

    private TcpClient _tcpClient;
    private Task _dataForwardingTask;
    private bool _hasBeenDisposed;
    private Task _messageReceptionTask;

    #endregion Fields involved in object disposal

    public NodeDescriptor Descriptors
    {
      get { return _descriptors; }
    }

    public HashSet<short> NeededData
    {
      get { return _neededData; }
    }

    public IBlockingCollection<DataChunkBase> ReceivedDataChunks
    {
      get { return _receivedDataChunks; }
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

    public ConnectionContainer(string cabInfoTypeDescriptorFilename = "Zusi3/CabInfoTypes.xml")
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

    public ConnectionContainer(NodeDescriptor rootDescriptor)
    {
      InitialiseFrom(rootDescriptor);
    }

    private void InitialiseFrom(Stream fileStream)
    {
      var cabInfoDescriptors = DescriptorReader.ReadCommandsetFrom(fileStream);
      InitialiseFrom(cabInfoDescriptors);
    }

    private void InitialiseFrom(NodeDescriptor rootDescriptor)
    {
      _descriptors = rootDescriptor;

      SetupNodeConverters();
    }

    private void SetupNodeConverters()
    {
      var handshakeConverter = new NodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter.SubNodeConverters[0x02] = ackHelloConverter;

      var cabDataConverter = GenerateNodeConverter(_descriptors);
      var userDataConverter = new NodeConverter();
      userDataConverter.SubNodeConverters[0x04] = ackNeededDataConverter;
      userDataConverter.SubNodeConverters[0x0A] = cabDataConverter;

      _rootNodeConverter = new RootNodeConverter();
      _rootNodeConverter[0x01] = handshakeConverter;
      _rootNodeConverter[0x02] = userDataConverter;
    }

    private INodeConverter GenerateNodeConverter(NodeDescriptor nodeDescriptor)
    {
      try
      {
        var attributeConverters = MapAttributeConverters(nodeDescriptor.AttributeDescriptors);
        Dictionary<short, INodeConverter> nodeConverters = nodeDescriptor.NodeDescriptors.ToDictionary(descriptor => descriptor.Id, GenerateNodeConverter);
        return new NodeConverter() { ConversionFunctions = attributeConverters, SubNodeConverters = nodeConverters };
      }
      catch (Exception e)
      {
        throw new InvalidOperationException(String.Format("Error while processing node 0x{0:x4} - {1}", nodeDescriptor.Id, nodeDescriptor.Name), e);
      }
    }

    private Dictionary<short, Func<Address, byte[], IProtocolChunk>> MapAttributeConverters(IEnumerable<AttributeDescriptor> cabInfoDescriptors)
    {
      Dictionary<short, Func<Address, byte[], IProtocolChunk>> dictionary = new Dictionary<short, Func<Address, byte[], IProtocolChunk>>();

      foreach (var descriptor in cabInfoDescriptors)
      {
        try
        {
          dictionary.Add(descriptor.Id, _converterMap[descriptor.Type]);
        }
        catch (KeyNotFoundException e)
        {
          throw new InvalidDescriptorException(String.Format("Could not found converter for type '{0}', used in descriptor 0x{1:x4} - {2}.", descriptor.Type, descriptor.Id, descriptor.Name), e);
        }
      }

      return dictionary;
    }

    public void RequestData(params DescriptorBase[] descriptors)
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

      if (_messageReceptionTask != null && !_messageReceptionTask.Wait(500))
        throw new TimeoutException("Failed to shut down message recption task within timeout.");
      _messageReceptionTask = null;

      if (_dataForwardingTask != null && !_dataForwardingTask.Wait(500))
        throw new TimeoutException("Failed to shut down message forwarding task within timeout.");
      _dataForwardingTask = null;

      if (_tcpClient != null)
        _tcpClient.Close();

      _receivedDataChunks.CompleteAdding();
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

      _dataForwardingTask = Task.Run(() =>
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
          _receivedDataChunks.Add((DataChunkBase)protocolChunk);
        }
      });
    }
  }

  internal enum Zugart
  {
  }
}