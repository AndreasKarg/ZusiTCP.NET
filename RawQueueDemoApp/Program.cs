using System;
using ZusiTcpInterface;
using ZusiTcpInterface.Enums;

namespace RawQueueDemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine("Connecting...");

      var connectionCreator = new ConnectionCreator();

      var velocityAddress = connectionCreator.Descriptors["Geschwindigkeit"].Address;
      var gearboxPilotLightAddress = connectionCreator.Descriptors["LM Getriebe"].Address;
      var sifaPilotLightAddress = connectionCreator.Descriptors["Status Sifa-Leuchtmelder"].Address;
      var sifaHornAddress = connectionCreator.Descriptors["Status Sifa-Hupe"].Address;

      connectionCreator.NeededData.Request(velocityAddress, gearboxPilotLightAddress, sifaHornAddress, sifaHornAddress);

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