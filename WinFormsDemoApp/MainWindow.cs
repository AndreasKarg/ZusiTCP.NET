using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.DOM;
using ZusiTcpInterface.Zusi3.Enums;

namespace WinFormsDemoApp
{
  public partial class MainWindow : Form
  {
    private readonly ConnectionCreator _connectionCreator;
    private readonly ThreadMarshallingZusiDataReceiver _dataReceiver;

    public MainWindow()
    {
      InitializeComponent();

      _connectionCreator = new ConnectionCreator();

      _dataReceiver = new ThreadMarshallingZusiDataReceiver(_connectionCreator, SynchronizationContext.Current);

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
      Connection tempQualifier = _connectionCreator.Connection;
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

      var handshaker = new Handshaker(tempQualifier._receivedChunks, binaryWriter, ClientType.ControlDesk, "Win-Forms demo app", "1.0.0.0",
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

      lblConnecting.Text = "Connected!";
    }

    private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
    {
      _dataReceiver.Dispose();
      _connectionCreator.Dispose();
    }
  }
}