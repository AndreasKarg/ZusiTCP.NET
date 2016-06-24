using System;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Enums;

namespace DemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine("Connecting...");

      using (var connectionContainer = new ConnectionContainer())
      {
        var velocityDescriptor = connectionContainer.Descriptors["Geschwindigkeit"];
        var gearboxPilotLightDescriptor = connectionContainer.Descriptors["LM Getriebe"];
        var sifaPilotLightDescriptor = connectionContainer.Descriptors["Status Sifa-Leuchtmelder"];
        var sifaHornDescriptor = connectionContainer.Descriptors["Status Sifa-Hupe"];
        var neededData = new List<CabInfoAddress> { velocityDescriptor.Address, gearboxPilotLightDescriptor.Address, sifaPilotLightDescriptor.Address, sifaHornDescriptor.Address };
        connectionContainer.Connect("Raw queue demo app", "1.0.0.0", neededData);

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          DataChunkBase chunk;
          bool chunkTaken = connectionContainer.ReceivedDataChunks.TryTake(out chunk, 100);
          if(!chunkTaken)
            continue;

          var velocityAddress = velocityDescriptor.Address;
          var gearboxPilotLightAddress = gearboxPilotLightDescriptor.Address;
          var sifaPilotLightAddress = sifaPilotLightDescriptor.Address;
          var sifaHornAddress = sifaHornDescriptor.Address;

          if (chunk.Address == velocityAddress)
          {
            Console.WriteLine("Velocity [km/h] = {0}", ((DataChunk<Single>) chunk).Payload*3.6f);
          }
          else if (chunk.Address == gearboxPilotLightAddress)
          {
            Console.WriteLine("Gearbox pilot light = {0}", ((DataChunk<bool>) chunk).Payload);
          }
          else if (chunk.Address == sifaPilotLightAddress)
          {
            Console.WriteLine("Sifa pilot light = {0}", ((DataChunk<bool>)chunk).Payload);
          }
          else if (chunk.Address == sifaHornAddress)
          {
            Console.WriteLine("Sifa horn state = {0}", ((DataChunk<StatusSifaHupe>)chunk).Payload);
          }
        }
      }

      Console.WriteLine("Disconnected");
    }
  }
}