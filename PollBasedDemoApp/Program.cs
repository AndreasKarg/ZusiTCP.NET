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
        var sifaPilotLightDescriptor = connectionContainer.Descriptors["Status Sifa:Status Sifa-Leuchtmelder"];
        var sifaHornDescriptor = connectionContainer.Descriptors["Status Sifa-Hupe"];

        var neededData = new List<CabInfoAddress> { velocityDescriptor.Address, gearboxPilotLightDescriptor.Address, sifaPilotLightDescriptor.Address };
        connectionContainer.Connect("Poll-based demo app", "1.0.0.0", neededData);

        var polledDataReceiver = new PolledZusiDataReceiver(connectionContainer);

        polledDataReceiver.RegisterCallbackFor<bool>("LM Getriebe", dataChunk => OnBoolReceived("LM Getriebe", dataChunk));
        polledDataReceiver.RegisterCallbackFor<bool>("Status Sifa-Leuchtmelder", dataChunk => OnBoolReceived("LM Sifa", dataChunk));
        polledDataReceiver.RegisterCallbackFor<StatusSifaHupe>("Status Sifa-Hupe", OnSifaHornReceived);
        polledDataReceiver.RegisterCallbackFor<float>("Geschwindigkeit", OnVelocityReceived);

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