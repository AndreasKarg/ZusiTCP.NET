using System;
using ZusiTcpInterface.Zusi3;

namespace DemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine("Connecting...");

      using (var connectionContainer = new ConnectionContainer())
      {
        var velocityDescriptor = connectionContainer.CabDataDescriptors.AttributeDescriptors["Geschwindigkeit"];
        var gearboxPilotLightDescriptor = connectionContainer.CabDataDescriptors.AttributeDescriptors["LM Getriebe"];
        var sifaStatusDescriptor = connectionContainer.CabDataDescriptors.AttributeDescriptors["Status Sifa"];
        connectionContainer.RequestData(velocityDescriptor, gearboxPilotLightDescriptor, sifaStatusDescriptor);
        connectionContainer.Connect();

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          CabDataChunkBase chunk;
          bool chunkTaken = connectionContainer.ReceivedCabDataChunks.TryTake(out chunk, 100);
          if(!chunkTaken)
            continue;

          switch (chunk.Id)
          {
            case 0x01:
              Console.WriteLine("Velocity [km/h] = {0}", ((CabDataChunk<Single>)chunk).Payload*3.6f);
              break;

            case 0x1A:
              Console.WriteLine("Gearbox pilot light = {0}", ((CabDataChunk<bool>)chunk).Payload);
              break;

            case 0x64:
              Console.WriteLine("Sifa status = {0}", ((CabDataChunk<SifaStatus>)chunk).Payload);
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