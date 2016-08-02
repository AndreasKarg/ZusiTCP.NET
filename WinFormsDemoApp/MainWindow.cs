using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using ZusiTcpInterface;
using ZusiTcpInterface.Enums;

namespace WinFormsDemoApp
{
  public partial class MainWindow : Form
  {
    private readonly ConnectionCreator _connectionCreator;
    private ThreadMarshallingZusiDataReceiver _dataReceiver;
    private Connection _connection;

    public MainWindow()
    {
      InitializeComponent();

      _connectionCreator = new ConnectionCreator();
      _connectionCreator.NeededData.Request("Geschwindigkeit", "LM Getriebe", "Status Sifa-Leuchtmelder", "Status Sifa-Hupe");
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

      _connection = _connectionCreator.CreateConnection(_connectionCreator._endPoint);

      _dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionCreator.Descriptors, _connection.ReceivedDataChunks, SynchronizationContext.Current);
      _dataReceiver.RegisterCallbackFor<bool>("LM Getriebe", OnGearboxPilotLightReceived);
      _dataReceiver.RegisterCallbackFor<bool>("Status Sifa-Leuchtmelder", OnSifaPilotLightReceived);
      _dataReceiver.RegisterCallbackFor<StatusSifaHupe>("Status Sifa-Hupe", OnSifaHornReceived);
      _dataReceiver.RegisterCallbackFor<float>("Geschwindigkeit", OnVelocityReceived);

      lblConnecting.Text = "Connected!";
    }

    private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
    {
      _dataReceiver.Dispose();
      _connection.Dispose();
    }
  }
}