using System;
using System.Collections.Generic;
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
        var velocityDescriptor = connectionContainer.Descriptors.AttributeDescriptors["Geschwindigkeit"];
        var gearboxPilotLightDescriptor = connectionContainer.Descriptors.AttributeDescriptors["LM Getriebe"];
        var sifaStatusDescriptor = connectionContainer.Descriptors.NodeDescriptors["Status Sifa"];
        var neededData = new HashSet<short> { velocityDescriptor.Id, gearboxPilotLightDescriptor.Id, sifaStatusDescriptor.Id };
        connectionContainer.Connect("Raw queue demo app", "1.0.0.0", neededData);

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          DataChunkBase chunk;
          bool chunkTaken = connectionContainer.ReceivedDataChunks.TryTake(out chunk, 100);
          if(!chunkTaken)
            continue;

          var velocityAddress = new Address(0x01);
          var gearboxPilotLightAddress = new Address(0x1A);
          var sifaAddress = new Address(0x64);

          if (chunk.Address == velocityAddress)
          {
            Console.WriteLine("Velocity [km/h] = {0}", ((DataChunk<Single>) chunk).Payload*3.6f);
          }
          else if (chunk.Address == gearboxPilotLightAddress)
          {
            Console.WriteLine("Gearbox pilot light = {0}", ((DataChunk<bool>) chunk).Payload);
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