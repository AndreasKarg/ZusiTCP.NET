using System;
using System.Windows.Forms;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Zusi_Datenausgabe;

/* ZusiTCPDemoApp
 * This example shows basic usage of Andreas Karg's Zusi TCP interface for .Net.
 * It is published under the GNU General Public License v3.0. Base your own work on it, play around, do what you want. :-)
 * 
 * 
 * Using the interface requires three steps:
 * - Write one or more handler methods
 * - Create a ZusiTcpConn object, passing basic parameters
 * - Tell your ZusiTcpConn object what measures you want to receive
 * 
 * Everything else is explained below. */

namespace ZusiTCPDemoApp
{
  public partial class CMainWindow : Form
  {
    // We do want to have a ZusiTcpConn object, so here's the declaration
    private IZusiTcpClientConnection MyTCPConnection;
    private readonly IWindsorContainer _container;
    private IZusiTcpConnectionFactory _connectionFactory;

    public CMainWindow()
    {
      InitializeComponent();

      _container = BootstrapContainer();

      try
      {
        RichTextBox1.Rtf = Properties.Resources.BlockFreiWarnung;
      }
      catch { }

      // When the application window is created, we create our new connection class as well.
      //MyTCPConnection = new ZusiTcpClientConnection("Zusi TCP Demo 1", ClientPriority.Low, "commandset.xml");

      _connectionFactory = _container.Resolve<IZusiTcpConnectionFactory>();
      MyTCPConnection = _connectionFactory.Create("Zusi TCP Demo 1", ClientPriority.Low, "commandset.xml");
      //MyTCPConnection = new ZusiTcpClientConnectionNoWindsor("Zusi TCP Demo 1", ClientPriority.Low, "commandset.xml");

      MyTCPConnection.ErrorReceived       += TCPConnection_ErrorReceived;
      MyTCPConnection.Subscribe<bool>(TCPConnection_BoolReceived);
      MyTCPConnection.Subscribe<BrakeConfiguration>(TCPConnection_BrakeConfigReceived);
      MyTCPConnection.Subscribe<DateTime>(TCPConnection_DateTimeReceived);
      MyTCPConnection.Subscribe<DoorState>(TCPConnection_DoorsReceived);
      MyTCPConnection.Subscribe<float>(TCPConnection_FloatReceived);
      MyTCPConnection.Subscribe<int>(TCPConnection_IntReceived);
      MyTCPConnection.Subscribe<PZBSystem>(TCPConnection_PZBReceived);
      MyTCPConnection.Subscribe<string>(TCPConnection_StringReceived);


      // We need to tell our connection object what measures to request from Zusi.
      // You may either use Zusi's native ID code or plain text as listed in the server's commandset.xml        
      MyTCPConnection.RequestData<float>("Geschwindigkeit", SpeedReceived); //2561 . . . . . . . . . . . . .  => FloatReceived
      MyTCPConnection.RequestData(2576); // "Fahrstufe"  . . . . . . . . . . . . . . .  => FloatReceived        
      MyTCPConnection.RequestData(2610); // "Uhrzeit"  . . . . . . . . . . . . . . . .  => DateTimeReceived        
      MyTCPConnection.RequestData(2637); // "LM Block, bis zu dem die Strecke frei ist" => StringReceived        
      MyTCPConnection.RequestData(2649); // "PZB-System"   . . . . . . . . . . . . . .  => PZBReceived        
      MyTCPConnection.RequestData(2594); // "LM LZB Ü-System"  . . . . . . . . . . . .  => BoolReceived
    }

    private IWindsorContainer BootstrapContainer()
    {
      return new WindsorContainer()
        .Install(new WindsorInstaller()
        );
    }


    // This is what happens when the user clicks the "Connect" button.
    private void BtnConnect_Click(object sender, EventArgs e)
    {
      // If we're currently disconnected...
      if (MyTCPConnection.ConnectionState == Zusi_Datenausgabe.ConnectionState.Disconnected)
      {
        // ... try to ... 
        try
        {
          // ... establish a connection using the hostname and port number from the main window.
          MyTCPConnection.Connect(TbServer.Text, (int)Convert.ToInt32(TbPort.Text));

          // When successful, change the button label to "Disconnect".
          BtnConnect.Text = "Disconnect";
        }

        // If something goes wrong...
        catch (ZusiTcpException ex)
        {
          // ... show the user what the connection object has to say.
          MessageBox.Show(String.Format("An error occured when trying to connect: {0}", ex.Message));

          // ... reset the connection by explicitly calling Disconnect();
          MyTCPConnection.Disconnnect();

          // ... and then change the button label to "Connect".
          BtnConnect.Text = "Connect";
        }
      }

      // If we're currently connected or the connection fell into an error state...
      else
      {
        // ... reset the connection by explicitly calling Disconnect();
        MyTCPConnection.Disconnnect();

        // ... and then change the button label to "Connect".
        BtnConnect.Text = "Connect";
      }
    }


    private void TCPConnection_ErrorReceived(object sender, ErrorEventArgs args) // Handles MyTCPConnection.ErrorReceived
    {
      // If something goes wrong...
      // ... show the user what the connection object has to say.
      System.Windows.Forms.MessageBox.Show(String.Format("An error occured when receiving data: {0}", args.Exception.Message));

      // ... reset the connection by explicitly calling Disconnect()
      MyTCPConnection.Disconnnect();

      //... and then change the button label to "Connect". 
      BtnConnect.Text = "Connect";
    }

    private void TCPConnection_BoolReceived(object sender, DataReceivedEventArgs<bool> data) // Handles MyTCPConnection.BoolReceived
    {
      switch (data.Id)
      {
        case 2594: // "LM LZB Ü-System" => BoolReceived 
          lblPZB.Enabled = !data.Value; //PZB off when LZB is on 
          lblLZB.Enabled = data.Value; //enable LZB when it's on
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_BrakeConfigReceived(object sender, DataReceivedEventArgs<BrakeConfiguration> data) // Handles MyTCPConnection.BrakeConfigReceived
    {
      switch (data.Id)
      {
        default:
          // We didn't request a BrakeConfig-data  
          break;
      }
    }

    private void TCPConnection_DateTimeReceived(object sender, DataReceivedEventArgs<DateTime> data) // Handles MyTCPConnection.DateTimeReceived  
    {
      switch (data.Id)
      {
        case 2610: // "Uhrzeit" => DateTimeReceived     
          lblUhrzeit.Text = string.Format("Uhrzeit: {0}", data.Value.ToLongTimeString());
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_DoorsReceived(object sender, DataReceivedEventArgs<DoorState> data) // Handles MyTCPConnection.DoorsReceived  
    {
      switch (data.Id)
      {
        default:
          //We didn't request a Doors-data  
          break;
      }
    }

    private void TCPConnection_FloatReceived(object sender, DataReceivedEventArgs<float> data) // Handles MyTCPConnection.FloatReceived    
    {
      switch (data.Id)
      {
        case 2561: // "Geschwindigkeit" => FloatReceived    
          //SpeedReceived(sender, data);
          break;
        case 2576: // "Fahrstufe" => FloatReceived               
          lblFahrstufe.Text = String.Format("Fahrstufe: {0}", data.Value.ToString("0")); // no decimals     
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void SpeedReceived(object sender, DataReceivedEventArgs<float> data)
    {
      lblGeschw.Text = String.Format("Geschwindigkeit: {0} km/h ", data.Value.ToString("0.00")); // two decimals
    }

    private void TCPConnection_IntReceived(object sender, DataReceivedEventArgs<int> data) // Handles MyTCPConnection.IntReceived   
    {
      switch (data.Id)
      {
        default:
          //We didn't request a Integer-data    
          break;
      }
    }

    private void TCPConnection_PZBReceived(object sender, DataReceivedEventArgs<PZBSystem> data) // Handles MyTCPConnection.PZBReceived  
    {
      switch (data.Id)
      {
        case 2649: // "PZB-System" => PZBReceived        
          switch (data.Value)
          {
            case PZBSystem.None:
            case PZBSystem.SBBSignum:
              lblPZB.Text = "PZB: keine";
              lblLZB.Text = "LZB: keine";
              break;
            case PZBSystem.PZ80R:
            case PZBSystem.PZB90V15:
            case PZBSystem.PZB90V16:
              lblPZB.Text = "PZB: PZB 90";
              lblLZB.Text = "LZB: keine";
              break;
            case PZBSystem.IndusiH54:
            case PZBSystem.IndusiI54:
            case PZBSystem.IndusiI60:
              lblPZB.Text = "PZB: Indusi";
              lblLZB.Text = "LZB: keine";
              break;
            case PZBSystem.PZ80:
              lblPZB.Text = "PZB: PZ 80";
              lblLZB.Text = "LZB: keine";
              break;
            case PZBSystem.IndusiI60R:
              lblPZB.Text = "PZB: Indusi I60R";
              lblLZB.Text = "LZB: keine";
              break;
            case PZBSystem.LZB80I80:
              lblPZB.Text = "PZB: Indusi I60R";
              lblLZB.Text = "LZB: LZB 80";
              break;
            default:
              lblPZB.Text = "PZB: Zustand nicht erkannt";
              lblLZB.Text = "LZB: Zustand nicht erkannt";
              break;
          }
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_StringReceived(object sender, DataReceivedEventArgs<string> data) // Handles MyTCPConnection.StringReceived        
    {
      switch (data.Id)
      {
        case 2637: // "LM Block, bis zu dem die Strecke frei ist" => StringReceived    
          lblFreiBis.Text = String.Format("Frei bis: {0}", data.Value);
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }
  }
}
