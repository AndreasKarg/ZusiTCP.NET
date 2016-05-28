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

          var velocityAddress = new Address(0x01);
          var gearboxPilotLightAddress = new Address(0x1A);
          var sifaAddress = new Address(0x64);

          if (chunk.Id == velocityAddress)
          {
            Console.WriteLine("Velocity [km/h] = {0}", ((CabDataChunk<Single>) chunk).Payload*3.6f);
          }
          else if (chunk.Id == gearboxPilotLightAddress)
          {
            Console.WriteLine("Gearbox pilot light = {0}", ((CabDataChunk<bool>) chunk).Payload);
          }
          else if (chunk.Id == sifaAddress)
          {
            Console.WriteLine("Sifa status = {0}", ((CabDataChunk<SifaStatus>) chunk).Payload);
          }
          else
          {
            throw new NotSupportedException("lol u mad bro???");
          }
        }
      }

      Console.WriteLine("Disconnected");
    }
  }
}