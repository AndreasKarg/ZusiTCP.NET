using System;
using System.Collections.Generic;
using System.Threading;
using ZusiTcpInterface.Zusi3;

namespace PollBasedDemoApp
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

        var neededData = new List<short> { velocityDescriptor.Id, gearboxPilotLightDescriptor.Id, sifaStatusDescriptor.Id };
        connectionContainer.Connect("Poll-based demo app", "1.0.0.0", neededData);

        var polledDataReceiver = new PolledZusiDataReceiver(connectionContainer);
        polledDataReceiver.FloatReceived += PolledDataReceiverOnFloatReceived;
        polledDataReceiver.BoolReceived += PolledDataReceiverOnBoolReceived;

        Console.WriteLine("Connected!");

        while (!Console.KeyAvailable)
        {
          polledDataReceiver.Service();
          Thread.Sleep(100);
        }
      }

      Console.WriteLine("Disconnected");
    }

    private static void PolledDataReceiverOnBoolReceived(object sender, DataReceivedEventArgs<bool> dataReceivedEventArgs)
    {
      var descriptor = dataReceivedEventArgs.Descriptor;
      Console.WriteLine("{0} = {1}", descriptor.Name, dataReceivedEventArgs.Payload);
    }

    private static void PolledDataReceiverOnFloatReceived(object sender, DataReceivedEventArgs<float> dataReceivedEventArgs)
    {
      var descriptor = dataReceivedEventArgs.Descriptor;
      switch (descriptor.Name)
      {
        case "Geschwindigkeit":
          Console.WriteLine("Velocity [km/h] = {0}", dataReceivedEventArgs.Payload * 3.6f);
          break;

        default:
          Console.WriteLine("{0} [{1}] = {2}", descriptor.Name, descriptor.Unit, dataReceivedEventArgs.Payload * 3.6f);
          break;
      }
    }
  }
}