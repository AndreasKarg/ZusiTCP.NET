using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZusiTcpInterface.Zusi3;

namespace DemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      var neededData = new List<short>
      {
        0x01, 0x02
      };

      var cabDataConversionFunctions = new Dictionary<short, Func<short, byte[], IProtocolChunk>>
      {
        { 0x01, CabDataAttributeConverters.ConvertSingle},
        { 0x02, CabDataAttributeConverters.ConvertSingle},
      };

      var handshakeConverter = new BranchingNodeConverter();
      var ackHelloConverter = new AckHelloConverter();
      var ackNeededDataConverter = new AckNeededDataConverter();
      handshakeConverter[0x02] = ackHelloConverter;

      var cabDataConverter = new CabDataConverter(cabDataConversionFunctions);
      var userDataConverter = new BranchingNodeConverter();
      userDataConverter[0x04] = ackNeededDataConverter;
      userDataConverter[0x0A] = cabDataConverter;

      TopLevelNodeConverter topLevelNodeConverter = new TopLevelNodeConverter();
      topLevelNodeConverter[0x01] = handshakeConverter;
      topLevelNodeConverter[0x02] = userDataConverter;

      Console.WriteLine("Connecting...");

      using (var client = new TcpClient("localhost", 1436))
      {
        Console.WriteLine("Connected!");
        var networkStream = client.GetStream();
        var binaryReader = new BinaryReader(networkStream);
        var binaryWriter = new BinaryWriter(networkStream);

        var chunkRxQueue = new BlockingCollectionWrapper<IProtocolChunk>();

        var messageReader = new MessageReceiver(binaryReader, topLevelNodeConverter, chunkRxQueue);
        var messageLoop = Task.Run(() =>
        {
          while (true)
          {
            messageReader.ProcessNextPacket();
          }
        });

        var handshaker = new Handshaker(chunkRxQueue, binaryWriter, ClientType.ControlDesk, "Z3 Protocol Demo", "1.0",
          neededData);

        handshaker.ShakeHands();

        Console.WriteLine("Hands have been shaken.");

        while (true)
        {
          var chunk = (CabDataChunk<Single>) chunkRxQueue.Take();
          switch (chunk.Id)
          {
            case 0x01:
              Console.WriteLine("Velocity [km/h] = {0}", chunk.Payload*3.6f);
              break;

            case 0x02:
              Console.WriteLine("Main brake line pressure [bar] = {0}", chunk.Payload);
              break;

            default:
              throw new NotSupportedException("Lol?");
          }
        }
      }
    }
  }
}