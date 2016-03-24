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

      using (var connectionContainer = new ConnectionContainer())
      {
        connectionContainer.RequestData("Geschwindigkeit");
        connectionContainer.RequestData("LM Getriebe");
        connectionContainer.Connect();

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          var chunk = connectionContainer.ReceivedCabDataChunks.Take();
          switch (chunk.Id)
          {
            case 0x01:
              Console.WriteLine("Velocity [km/h] = {0}", ((CabDataChunk<Single>)chunk).Payload*3.6f);
              break;

            case 0x1A:
              Console.WriteLine("Gearbox pilot light = {0}", ((CabDataChunk<bool>)chunk).Payload);
              break;

            default:
              throw new NotSupportedException("lol u mad bro???");
          }
        }
      }

      Console.WriteLine("Disconnected");
    }
  }
}