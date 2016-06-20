using System;
using System.Collections.Generic;
using System.Threading;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Enums;

namespace PollBasedDemoApp
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
        var sifaStatusDescriptor = connectionContainer.Descriptors["Status Sifa"];

        var neededData = new List<CabInfoAddress> { velocityDescriptor.Address, gearboxPilotLightDescriptor.Address, sifaStatusDescriptor.Address };
        connectionContainer.Connect("Poll-based demo app", "1.0.0.0", neededData);

        var polledDataReceiver = new PolledZusiDataReceiver(connectionContainer);

        polledDataReceiver.RegisterCallbackFor<bool>(new CabInfoAddress(0x1A), dataChunk => OnBoolReceived("LM Getriebe", dataChunk));
        polledDataReceiver.RegisterCallbackFor<bool>(new CabInfoAddress(0x64, 0x02), dataChunk => OnBoolReceived("LM Sifa", dataChunk));
        polledDataReceiver.RegisterCallbackFor<StatusSifaHupe>(new CabInfoAddress(0x64, 0x03), OnSifaHornReceived);
        polledDataReceiver.RegisterCallbackFor<float>(new CabInfoAddress(0x01), OnVelocityReceived);

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          polledDataReceiver.Service();
          Thread.Sleep(100);
        }
      }

      Console.WriteLine("Disconnected");
    }

    private static void OnSifaHornReceived(DataChunk<StatusSifaHupe> dataChunk)
    {
      Console.WriteLine("Sifa-Horn = {0}", dataChunk.Payload);

    }

    private static void OnVelocityReceived(DataChunk<float> dataChunk)
    {
      Console.WriteLine("Velocity [km/h] = {0}", dataChunk.Payload * 3.6f);
    }

    private static void OnBoolReceived(string name, DataChunk<bool> dataChunk)
    {
      Console.WriteLine("{0} = {1}", name, dataChunk.Payload);
    }
  }
}
