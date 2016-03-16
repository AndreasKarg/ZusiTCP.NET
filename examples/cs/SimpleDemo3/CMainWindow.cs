using System;
using System.Windows.Forms;
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
    private ZusiTcp3TypeClient MyTCPConnection;

    public CMainWindow()
    {
      InitializeComponent();

      // When the application window is created, we create our new connection class as well.
      MyTCPConnection = new ZusiTcp3TypeClient("Zusi TCP Demo 1", "1.0", "commandset3.xml");

      MyTCPConnection.StringReceived       += TCPConnection_StringReceived;
      MyTCPConnection.FloatReceived        += TCPConnection_FloatReceived;
      MyTCPConnection.DateTimeReceived     += TCPConnection_DateTimeReceived;
      MyTCPConnection.BoolReceived         += TCPConnection_BoolReceived;
      MyTCPConnection.IntReceived          += TCPConnection_IntReceived;
      MyTCPConnection.DoorsReceived        += TCPConnection_DoorsReceived;
      MyTCPConnection.BrakeConfigReceived  += TCPConnection_BrakeConfigReceived;
      MyTCPConnection.ErrorReceived        += TCPConnection_ErrorReceived;
      MyTCPConnection.ZugsicherungReceived += TCPConnection_ZugsicherungReceived;
      this.BtnPause.Click   += BtnPause_Click;
      this.BtnSpeed.Click   += BtnSpeed_Click;
      this.BtnJump.Click    += BtnJump_Click;
      this.BtnAP.Click      += BtnAP_Click;
      this.BtnTexture.Click += BtnTexture_Click;


      // We need to tell our connection object what measures to request from Zusi.
      // You may either use Zusi's native ID code or plain text as listed in the server's commandset.xml        
      MyTCPConnection.RequestData("Geschwindigkeit"); //0x1 (m/s)  . . . . . . . . . .  => FloatReceived        
      MyTCPConnection.RequestData(0x15); // "Fahrstufe"  . . . . . . . . . . . . . . .  => FloatReceived        
      MyTCPConnection.RequestData(0x23); // "Uhrzeit"  . . . . . . . . . . . . . . . .  => DateTimeReceived        
      MyTCPConnection.RequestData(0x4B); // "Datum"  . . . . . . . . . . . . . . . . .  => DateTimeReceived        
      //MyTCPConnection.RequestData(2637); // "LM Block, bis zu dem die Strecke frei ist" => StringReceived        
      MyTCPConnection.RequestData(0x65); // "Zugsicherung"   . . . . . . . . . . . . .  => PZBReceived        
      //MyTCPConnection.RequestData(2594); // "LM LZB Ü-System"  . . . . . . . . . . . .  => BoolReceived
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
          MyTCPConnection.Disconnect();

          // ... and then change the button label to "Connect".
          BtnConnect.Text = "Connect";
        }
      }

      // If we're currently connected or the connection fell into an error state...
      else
      {
        // ... reset the connection by explicitly calling Disconnect();
        MyTCPConnection.Disconnect();

        // ... and then change the button label to "Connect".
        BtnConnect.Text = "Connect";
      }
    }


    private void TCPConnection_ErrorReceived(object sender, ZusiTcpException ex) // Handles MyTCPConnection.ErrorReceived
    {
      // If something goes wrong...
      // ... show the user what the connection object has to say.
      System.Windows.Forms.MessageBox.Show(String.Format("An error occured when receiving data: {0}", ex.Message));

      // ... reset the connection by explicitly calling Disconnect()
      MyTCPConnection.Disconnect();

      //... and then change the button label to "Connect". 
      BtnConnect.Text = "Connect";
    }

    private void TCPConnection_BoolReceived(object sender, DataSet<bool> data) // Handles MyTCPConnection.BoolReceived
    {
      switch (data.Id)
      {
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_BrakeConfigReceived(object sender, DataSet<BrakeConfiguration> data) // Handles MyTCPConnection.BrakeConfigReceived
    {
      switch (data.Id)
      {
        default:
          // We didn't request a BrakeConfig-data  
          break;
      }
    }

    private void TCPConnection_DateTimeReceived(object sender, DataSet<DateTime> data) // Handles MyTCPConnection.DateTimeReceived  
    {
      switch (data.Id)
      {
        case 0x23: // "Uhrzeit" => DateTimeReceived     
          lblUhrzeit.Text = string.Format("Uhrzeit: {0}", data.Value.ToLongTimeString());
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_DoorsReceived(object sender, DataSet<DoorState> data) // Handles MyTCPConnection.DoorsReceived  
    {
      switch (data.Id)
      {
        default:
          //We didn't request a Doors-data  
          break;
      }
    }

    private void TCPConnection_FloatReceived(object sender, DataSet<float> data) // Handles MyTCPConnection.FloatReceived    
    {
      switch (data.Id)
      {
        case 0x1: // "Geschwindigkeit" => FloatReceived    
          lblGeschw.Text = String.Format("Geschwindigkeit: {0} km/h ", (data.Value * 3.6).ToString("0.00")); // two decimals  
          break;
        case 0x15: // "Fahrstufe" => FloatReceived               
          lblFahrstufe.Text = String.Format("Fahrstufe: {0}", data.Value.ToString("0")); // no decimals     
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_IntReceived(object sender, DataSet<int> data) // Handles MyTCPConnection.IntReceived   
    {
      switch (data.Id)
      {
        default:
          //We didn't request a Integer-data    
          break;
      }
    }

    private void TCPConnection_StringReceived(object sender, DataSet<string> data) // Handles MyTCPConnection.StringReceived        
    {
      switch (data.Id)
      {
        //case 2637: // "LM Block, bis zu dem die Strecke frei ist" => StringReceived    
        //  lblFreiBis.Text = String.Format("Frei bis: {0}", data.Value);
        //  break;
        default:
          // For unknown IDs...  
          break;
      }
    }

    private void TCPConnection_ZugsicherungReceived(object sender, DataSet<Zusi_Datenausgabe.Zugsicherung> data) // Handles MyTCPConnection.FloatReceived    
    {
      switch (data.Id)
      {
        case 0x65: // "Geschwindigkeit" => FloatReceived    
          string s32 = data.Value.ZugsicherungName;
          s32 = s32.Replace("PZB", ":::PZB");
          s32 = s32.Replace("LZB", ":::LZB");
          string[] s33 = s32.Split(new string[] {":::"}, System.StringSplitOptions.RemoveEmptyEntries);
          string pzb = "PZB: Ohne";
          string lzb = "LZB: Ohne";
          foreach (string s0 in s33)
          {
              string s1 = s0;
              if (s1.Contains("PZB") || s1.Contains("Indusi"))
                  pzb = "PZB: " + s1;
              else if (s1.Contains("LZB"))
                  lzb = "LZB: " + s1;
              else
              {
                  pzb += " / " + s1;
                  lzb += " / " + s1;
                  s1 = s1;
              }
          }
          lblPZB.Text = pzb;
          lblLZB.Text = lzb;
          lblPZB.Enabled = !data.Value.StateIndusi.LM_Ue; //PZB off when LZB is on
          lblLZB.Enabled = data.Value.StateIndusi.LM_Ue; //enable LZB when it's on
          break;
        default:
          // For unknown IDs...  
          break;
      }
    }
    
    private void BtnPause_Click(object sender, System.EventArgs e) //Handles BtnPause.Click
    {
        if (MyTCPConnection == null)
          return;
        MyTCPConnection.SendPause(Zusi_Datenausgabe.SwitchAction.Toogle);
    }

    private void BtnSpeed_Click(object sender, System.EventArgs e) //Handles BtnSpeed.Click
    {
        if (MyTCPConnection == null)
          return;
        MyTCPConnection.SendZeitraffer(Zusi_Datenausgabe.SwitchAction.Toogle);
    }

    private void BtnJump_Click(object sender, System.EventArgs e) //Handles BtnJump.Click
    {
        if (MyTCPConnection == null)
          return;
        MyTCPConnection.SendZeitsprung(Zusi_Datenausgabe.SwitchAction.Toogle);
    }

    private void BtnAP_Click(object sender, System.EventArgs e) //Handles BtnAP.Click
    {
        if (MyTCPConnection == null)
          return;
        MyTCPConnection.SendKIActive(Zusi_Datenausgabe.SwitchAction.Toogle); //FAIL
    }

    private void BtnTexture_Click(object sender, System.EventArgs e) //Handles BtnTexture.Click
    {
        if (MyTCPConnection == null)
          return;
        var b32 = new System.Drawing.Bitmap(1000, 1000);
        this.DrawToBitmap(b32, new System.Drawing.Rectangle(0, 0, 1000, 1000));
        var memstr = new System.IO.MemoryStream();
        b32.Save(memstr, System.Drawing.Imaging.ImageFormat.Bmp);
        MyTCPConnection.SendTexture(memstr.ToArray(),
            0, "DAVID_rechts", 0, 0);
    }
  }
}
