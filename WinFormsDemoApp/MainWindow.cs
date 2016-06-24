using System;
using System.Threading;
using System.Windows.Forms;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.Enums;

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

      _dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionContainer, SynchronizationContext.Current);

      _dataReceiver.RegisterCallbackFor<bool>("LM Getriebe", OnGearboxPilotLightReceived);
      _dataReceiver.RegisterCallbackFor<bool>("Status Sifa-Leuchtmelder", OnSifaPilotLightReceived);
      _dataReceiver.RegisterCallbackFor<StatusSifaHupe>("Status Sifa-Hupe", OnSifaHornReceived);
      _dataReceiver.RegisterCallbackFor<float>("Geschwindigkeit", OnVelocityReceived);
    }

    private void OnGearboxPilotLightReceived(DataChunk<bool> dataChunk)
    {
      lblGearboxIndicator.Text = dataChunk.Payload.ToString();
    }

    private void OnSifaPilotLightReceived(DataChunk<bool> dataChunk)
    {
      lblSifaStatus.Text = dataChunk.Payload.ToString();
    }

    private void OnVelocityReceived(DataChunk<float> dataChunk)
    {
      lblVelocity.Text = String.Format("{0:F1}", dataChunk.Payload * 3.6f);
    }

    private void OnSifaHornReceived(DataChunk<StatusSifaHupe> dataChunk)
    {
      lblSifaHorn.Text = dataChunk.Payload.ToString();
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
      lblConnecting.Text = "Connecting!";

      var neededData = new[] { "Geschwindigkeit", "LM Getriebe", "Status Sifa-Leuchtmelder", "Status Sifa-Hupe" };
      _connectionContainer.Connect("Win-Forms demo app", "1.0.0.0", neededData);

      lblConnecting.Text = "Connected!";
    }

    private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
    {
      _dataReceiver.Dispose();
      _connectionContainer.Dispose();
    }
  }
}