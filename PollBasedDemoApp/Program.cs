using System;
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
        var velocityDescriptor = connectionContainer.CabDataDescriptors.AttributeDescriptors["Geschwindigkeit"];
        var gearboxPilotLightDescriptor = connectionContainer.CabDataDescriptors.AttributeDescriptors["LM Getriebe"];
        var sifaStatusDescriptor = connectionContainer.CabDataDescriptors.NodeDescriptors["Status Sifa"];
        connectionContainer.RequestData(velocityDescriptor, gearboxPilotLightDescriptor, sifaStatusDescriptor);
        connectionContainer.Connect();

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