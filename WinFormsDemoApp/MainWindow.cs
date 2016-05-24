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
      var velocityDescriptor = _connectionContainer.Descriptors["Geschwindigkeit"];
      var gearboxPilotLightDescriptor = _connectionContainer.Descriptors["LM Getriebe"];
      var sifaStatusDescriptor = _connectionContainer.Descriptors["Status Sifa"];
      _connectionContainer.RequestData(velocityDescriptor, gearboxPilotLightDescriptor, sifaStatusDescriptor);

      _dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionContainer, SynchronizationContext.Current);
      _dataReceiver.FloatReceived += OnFloatReceived;
      _dataReceiver.BoolReceived += OnBoolReceived;
      _dataReceiver.SifaStatusReceived += OnSifaStatusReceived;
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

    private void OnSifaStatusReceived(object sender, DataReceivedEventArgs<SifaStatus> dataReceivedEventArgs)
    {
      if (dataReceivedEventArgs.Descriptor.Name != "Status Sifa")
        return;

      lblSifaStatus.Text = dataReceivedEventArgs.Payload.ToString();
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