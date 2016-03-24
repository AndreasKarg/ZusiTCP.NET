using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ZusiTcpInterface.Zusi3
{
  public class ConnectionContainer : IDisposable
  {
    private readonly DescriptorCollection _descriptors;
    private readonly HashSet<short> _neededData = new HashSet<short>();
    private static TopLevelNodeConverter _topLevelNodeConverter;
    private readonly IBlockingCollection<CabDataChunkBase> _blockingCollection = new BlockingCollectionWrapper<CabDataChunkBase>();
    private TcpClient _tcpClient = null;

    public DescriptorCollection Descriptors
    {
      get { return _descriptors; }
    }

    public HashSet<short> NeededData
    {
      get { return _neededData; }
    }

    public IBlockingCollection<CabDataChunkBase> ReceivedChunkQueue
    {
      get { return _blockingCollection; }
    }

    public ConnectionContainer(string cabInfoTypeDescriptorFilename = "Zusi3/CabInfoTypes.csv")
    {
      List<CabInfoTypeDescriptor> cabInfoDescriptors;
      using (var commandSetFileStream = File.OpenRead(cabInfoTypeDescriptorFilename))
      {
        cabInfoDescriptors = CabInfoTypeDescriptorReader.ReadCommandsetFrom(commandSetFileStream).ToList();
      }

      _descriptors = new DescriptorCollection(cabInfoDescriptors);
      var cabInfoConversionFunctions = GenerateConversionFunctions(cabInfoDescriptors);

      SetupNodeConverters(cabInfoConversionFunctions);
    }

    private static void SetupNodeConverters(Dictionary<short, Func<short, byte[], IProtocolChunk>> cabInfoConversionFunctions)
    {
      var handshakeConverter = new BranchingNodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter[0x02] = ackHelloConverter;

      var cabDataConverter = new CabDataConverter(cabInfoConversionFunctions);
      var userDataConverter = new BranchingNodeConverter();
      userDataConverter[0x04] = ackNeededDataConverter;
      userDataConverter[0x0A] = cabDataConverter;

      _topLevelNodeConverter = new TopLevelNodeConverter();
      _topLevelNodeConverter[0x01] = handshakeConverter;
      _topLevelNodeConverter[0x02] = userDataConverter;
    }

    private Dictionary<short, Func<short, byte[], IProtocolChunk>> GenerateConversionFunctions(IEnumerable<CabInfoTypeDescriptor> cabInfoDescriptors)
    {
      var descriptorToConversionFunctionMap = new Dictionary<string, Func<short, byte[], IProtocolChunk>>()
      {
        {"single", CabDataAttributeConverters.ConvertSingle},
        {"boolassingle", CabDataAttributeConverters.ConvertBoolAsSingle},
        {"fail", (s, bytes) => {throw new NotSupportedException("Unsupported data type received");} }
      };

      return cabInfoDescriptors.ToDictionary(descriptor => descriptor.Id,
                                             descriptor => descriptorToConversionFunctionMap[descriptor.Type.ToLowerInvariant()]);
    }

    public void RequestData(string name)
    {
      _neededData.Add(_descriptors.GetBy(name).Id);
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public void Connect(string hostname = "localhost", int port = 1436)
    {
      _tcpClient = new TcpClient(hostname, port);

      var networkStream = _tcpClient.GetStream();
      var binaryReader = new BinaryReader(networkStream);
      var binaryWriter = new BinaryWriter(networkStream);

      var chunkRxQueue = new BlockingCollectionWrapper<IProtocolChunk>();

      var messageReader = new MessageReceiver(binaryReader, _topLevelNodeConverter, chunkRxQueue);
      var messageLoop = Task.Run(() =>
      {
        while (true)
        {
          messageReader.ProcessNextPacket();
        }
      });

      var handshaker = new Handshaker(chunkRxQueue, binaryWriter, ClientType.ControlDesk, "Z3 Protocol Demo", "1.0",
        _neededData);

      handshaker.ShakeHands();

      var secondMessageLoop = Task.Run(() =>
      {
        while (true)
        {
          _blockingCollection.Add((CabDataChunkBase)chunkRxQueue.Take());
        }
      });
    }
  }
}