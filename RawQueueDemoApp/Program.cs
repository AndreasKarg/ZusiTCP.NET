using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Enums;

namespace DemoApp
{
  internal static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine("Connecting...");

      using (var connectionContainer = new ConnectionCreator())
      {
        var velocityDescriptor = connectionContainer.Descriptors["Geschwindigkeit"];
        var gearboxPilotLightDescriptor = connectionContainer.Descriptors["LM Getriebe"];
        var sifaPilotLightDescriptor = connectionContainer.Descriptors["Status Sifa-Leuchtmelder"];
        var sifaHornDescriptor = connectionContainer.Descriptors["Status Sifa-Hupe"];
        var neededData = new[] {"Geschwindigkeit", "LM Getriebe", "Status Sifa-Leuchtmelder", "Status Sifa-Hupe"};
        Connection tempQualifier = connectionContainer.Connection;
        tempQualifier._tcpClient = new TcpClient("localhost", 1436);

        var networkStream = new CancellableBlockingStream(tempQualifier._tcpClient.GetStream(), tempQualifier._cancellationTokenSource.Token);
        var binaryReader = new BinaryReader(networkStream);
        var binaryWriter = new BinaryWriter(networkStream);

        var messageReader = new MessageReceiver(binaryReader, tempQualifier._rootNodeConverter, tempQualifier._receivedChunks);
        tempQualifier._messageReceptionTask = Task.Run(() =>
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

        var handshaker = new Handshaker(tempQualifier._receivedChunks, binaryWriter, ClientType.ControlDesk, "Raw queue demo app", "1.0.0.0",
          (IEnumerable<CabInfoAddress>) neededData);

        handshaker.ShakeHands();

        tempQualifier._dataForwardingTask = Task.Run(() =>
        {
          while (true)
          {
            IProtocolChunk protocolChunk;
            try
            {
              const int noTimeout = -1;
              tempQualifier._receivedChunks.TryTake(out protocolChunk, noTimeout, tempQualifier._cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
              // Teardown requested
              return;
            }
            tempQualifier._receivedDataChunks.Add((DataChunkBase)protocolChunk);
          }
        });

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          DataChunkBase chunk;
          bool chunkTaken = connectionContainer.Connection.ReceivedDataChunks.TryTake(out chunk, 100);
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