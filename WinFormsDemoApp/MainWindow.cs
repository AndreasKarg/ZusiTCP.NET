using System;
using System.Threading;
using System.Windows.Forms;
using ZusiTcpInterface.Zusi3;

namespace WinFormsDemoApp
{
  public partial class MainWindow : Form
  {
    private readonly ConnectionContainer _connectionContainer;
    private readonly ThreadMarshallingZusiDataReceiver _dataReceiver;

    public MainWindow()
    {
      InitializeComponent();

      _connectionContainer = new ConnectionContainer();
      var velocityDescriptor = _connectionContainer.Descriptors.AttributeDescriptors["Geschwindigkeit"];
      var gearboxPilotLightDescriptor = _connectionContainer.Descriptors.AttributeDescriptors["LM Getriebe"];
      var sifaStatusDescriptor = _connectionContainer.Descriptors.NodeDescriptors["Status Sifa"];
      _connectionContainer.RequestData(velocityDescriptor, gearboxPilotLightDescriptor, sifaStatusDescriptor);

      _dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionContainer, SynchronizationContext.Current);
      _dataReceiver.FloatReceived += OnFloatReceived;
      _dataReceiver.BoolReceived += OnBoolReceived;
    }

    private void OnBoolReceived(object sender, DataReceivedEventArgs<bool> e)
    {
      if (e.Descriptor.Name != "LM Getriebe")
        return;

      lblGearboxIndicator.Text = e.Payload.ToString();
    }

    private void OnFloatReceived(object sender, DataReceivedEventArgs<float> dataReceivedEventArgs)
    {
      if (dataReceivedEventArgs.Descriptor.Name != "Geschwindigkeit")
        return;

      lblVelocity.Text = String.Format("{0:F1}", dataReceivedEventArgs.Payload * 3.6f);
    }

    private void MainWindow_Load(object sender, System.EventArgs e)
    {
      lblConnecting.Text = "Connecting!";
      _connectionContainer.Connect();
      lblConnecting.Text = "Connected!";
    }

    private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
    {
      _dataReceiver.Dispose();
      _connectionContainer.Dispose();
    }
  }
}