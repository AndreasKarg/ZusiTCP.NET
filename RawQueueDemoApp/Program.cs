using System;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Enums;

namespace DemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine("Connecting...");

      var connectionCreator = new ConnectionCreator();
      connectionCreator.ClientName = "Poll-based demo app";
      connectionCreator.ClientVersion = "1.0.0.0";
      connectionCreator.NeededData = new[] { new CabInfoAddress(0x01), new CabInfoAddress(0x64), new CabInfoAddress(0x1A), };

      //var neededData = new[] { "Geschwindigkeit", "LM Getriebe", "Status Sifa:Status Sifa-Leuchtmelder", "Status Sifa-Hupe" };

      var velocityAddress = connectionCreator.Descriptors["Geschwindigkeit"].Address;
      var gearboxPilotLightAddress = connectionCreator.Descriptors["LM Getriebe"].Address;
      var sifaPilotLightAddress = connectionCreator.Descriptors["Status Sifa-Leuchtmelder"].Address;
      var sifaHornAddress = connectionCreator.Descriptors["Status Sifa-Hupe"].Address;

      using (var connection = connectionCreator.CreateConnection())
      {
        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          DataChunkBase chunk;
          bool chunkTaken = connection.ReceivedDataChunks.TryTake(out chunk, 100);
          if(!chunkTaken)
            continue;

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