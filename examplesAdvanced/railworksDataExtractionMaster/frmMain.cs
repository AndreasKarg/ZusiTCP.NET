/*
 * This program demonstrates how to use data extracted from Railworks / TS2014.
 * There is a file called Railworks_Getdata_Script.lua that needs to be placed
 * in the plugins folder inside Railworks. This script file extracts data from
 * Railworks in the format ControlName:setting ie TrainBrake:0 or Regulator:20
 * and places the data in a text file call GetData.txt which it also places
 * in the plugins folder. In order for this to happen you have to edit each
 * engine simulation script in Railworks so that it calls the Railworks_GetData_Script
 * 
 * See the PDF/DOC file Railworks_GetdData_Tutorial on how to do this.
 * 
 * This program will open the text file and read each line of data and display 
 * it in a textbox on the form and/or send selected data in the form of a string
 * to the serial port if one is selected. The data format for the serial port is
 * Control name ie TH for throttle, REV for reverser TB for train Brake etc 
 * followed by the value read from the GetData.txt file followed by a colon.
 * IE TH 50: for throttle 50% or REV 100: for reverser 100%.
 * I have then created a test circuit board using a Microchip (c) Pic Micro to 
 * read this data and diplay it on either a 4 line Graphics LCD display or a 
 * home made speedometer using an old analog alarm clock and a stepper motor.
 * I have commented the code throughout so hopefully it will make sence and
 * it should be easy enough to add your own commands if the ones I use aren't
 * enough, I have included another file called Railworks_Enginedata.pdf which
 * lists all the commands for the trains I own along with their minimum, maximum
 * and default values. Some of these value for the analog controls range from 
 * 0 to 1 or -1 to 1, In the Railworks_Getdata_Script I convert these to
 * 0 to 100 or -100 to 100 to give a percentage reading ie throttle 80% rather
 * than throttle 0.8%.
 * 
 * You are free to use or alter this program in any way you wish. It is written
 * in Microsoft Visual C# Express 2010 downloadable from
 * http://www.visualstudio.com/en-us/downloads/download-visual-studio-vs#DownloadFamilies_4
 * and uses Net Framework 3.5 which will be installed if not already installed
 * when you install c#.
 * 
 * For people who wish to display their own analog gauges on a second screen rather than
 * using physical gauges look here
 * http://www.codeproject.com/Articles/448721/AGauge-WinForms-Gauge-Control
 * I have experimented with an earlier version and it works quite well. See the dash.png
 * file.
 * 
 * Once the program is running if you wish to send data to the serial/com port you need to
 * select the com port to use from the cboComPort dropdown box and then check (tick) the
 * checkbox labeled Send output to Comm port. If you leave this unchecked and leave the
 * Send Output To Screen checkbox checked then all the data will stil be displayed in the textbox.
 * There is also a checkbox labeled Play Alerter Sound which when enabled and you are in the game
 * and the alerter sounds, you will hear the alerter no matter what view you are in.
 * I have set the program up to only send a maximum of 4 commands to the serial port selected
 * by checking a maximum of 4 items from the Select up to 4 items to display on GLCD checklistbox.
 * You will get a warning if you try to select more than 4. This can be changed in the code.
 * All you need to do next is click the start button to start gathering the data and obviously
 * the stop button to stop gathering the data.
 * 
 * Well I hope this program and the other files mention above help you get started with
 * your own DIY Trainsim Control Panel.
 * Good Luck
 * 
 * Chris Gamble chrisgamble1955@gmail.com
 *
 *
 * Note: This tutorial had been modifyed to be able to send Datas to a Zusi TCP Server
*/

using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;
using Zusi_Datenausgabe;

namespace Railworks_GetData

{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private StreamReader sr;//File reading
        private FileStream fs;  //File Reading
        private ZusiTcpTypeMaster tcp;
        private string railworksPath = ""; //Stores path to Railworks
        private string appPath = Application.StartupPath;//Location of this program
        private bool running = false; //Used to start / stop sending data to com port
        private bool SendToTcp = false; //Do we want to send data to the com port
        private bool SendToScreen = true;// Do we want to send data to the screen
        private string s = string.Empty; //Temp store for data
        private string comPort = string.Empty;//Name of com port selected
        private string AlertSound = Application.StartupPath + "\\Alerter.wav";//Sound file to play for alerter
        private System.Media.SoundPlayer player;//Sound player

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Get path to railworks
            railworksPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\railsimulator.com\railworks\", "Install_Path", "Not present");
            if (railworksPath == "Not Present")
            {
                MessageBox.Show("Cannot find Railworks installed on this machine", "Railworks Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            this.Text = "Data Extractor";
            btnStop.Enabled = false; //Disable stop button as program not running yet
            chkSendToPort.Enabled = false;//Disable sending to com port untill com port selected
            //Check to see if any com ports attached to machine
            foreach (string sp in new string[] {"192.168.178.", "192.168.1.", "localhost"})
            {
                //If so add them to the com port selection box
                cboComPort.Items.Add(sp);
            }
            //Load configuration data
            LoadConfiguration();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = true;//Program now runnig enable stop button
            btnStart.Enabled = false;//Disable start button
            cboComPort.Enabled = false;//Don't allow changing of com port while running
            string msg1 = "", tmp = "";
            string[] splitter;// Array of string to hold data read from GetData.txt
            running = true; //We are running
            bool bplaysound = false;//Don't play alerter sound yet.
            bool bplaying = false; //Is the alerter sound playing
//            int padding = 16;//Spaces used to align LCD display
            string ThrottleType = "";
//            string BrakeType = "";

            tcp = new ZusiTcpTypeMaster("Railworks");
            tcp.Connect(cboComPort.Text, 1435); //ToDo: Modify the UI to give the user the chance to modify the Port.
            
            //Start continous loop until stop is clicked
            while (running)
            {

//                //*****************************************************************
//                int lines = 0;
//                //Added so we can count the number of lines to send to the
//                //serial port so we don't get duplicates on the glcd.
//                //*****************************************************************
                
                //Check if we want to play alerter and that it isn't playing yet
                if (bplaysound == true && bplaying == false)
                {
                    player.PlayLooping(); //Start playing the alerter
                    
                    bplaying = true;//Update that we are playing the alerter.
                }
                //Check if we no longer need to play sound, if we don't and it is playing then stop it
                else if (bplaysound == false && bplaying == true)
                {
                    player.Stop(); //Stop the sound player
                    bplaying = false;//Update that it has stopped
                }
                //Check that the GetData.txt file exists in the plugins folder. This file is created by the Railworks_GetData_Script.lua script
                if (File.Exists(railworksPath + "\\plugins\\GetData.txt"))
                {                    
                    //The file does exist so open it for reading but with read & write access so Railworks can still write to it while we have it open.
                    fs = new FileStream(railworksPath + "\\plugins\\GetData.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    sr = new StreamReader(fs);
                    msg1 = "";
                    
                    label1.Text = "Status: Running";
                    
                    //Read each line in turn until end of file is reached
                    while (!sr.EndOfStream)
                    {
                        tmp = sr.ReadLine();//Store the read line into tmp variable
                        
                        //Each line read from the getdata.txt file will be in the form of control name followed by a colon followed by its setting as in Throttle:80
                        //The split function searches for the colon : and seperates the 2 strings and stores them in the splitter array.
                        splitter = tmp.Split(':');
                        //Check we have 2 pieces of data IE a control and value
                        if (splitter.Length == 2)
                        {
                            //Do we want to send data to the com port
                            if (SendToTcp)
                            {
                                //Get speedo type (MPH or KPH)
                                if (splitter[0] == "Speedo type" && splitter[1] != "None")
                                {
                                    s = splitter[1];
                                }

                                //Cycle through all (Max 4) checked items in items to display on GLCD checkedlistbox
                                foreach (object ItemToDisplay in clbItemsToDisplay.CheckedItems)
                                {
                                    //Compare control name read from GetData.txt (in splitter[0]) with each checked item in the checkedlistbox
                                    switch(splitter[0])
                                    {
                                        case "Current Speed":
                                            if(ItemToDisplay.ToString() == "Speed (Speed)")
                                            {
                                                float speed;
                                                if (!float.TryParse(splitter[1], out speed))
                                                    continue;

                                                tcp.SendSingle(speed, 2561);
                                            }
                                            break;
                                         //Check throttle type VirtualThrottle, ThrottleAndBrake or Regulator
                                        case "ThrottleType":
                                            ThrottleType = splitter[1];
                                            break;
                                        case "VirtualThrottle":
                                        case "ThrottleAndBrake":
                                        case "Regulator":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Throttle (TH)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "TH ", Value obtain following the colon from the getdata.txt
//                                                //plus % sign and enough spaces to make the text length 16 plus a colon
//                                                msg = ("TH " + splitter[1] + "%").PadRight(padding) + ":";
//                                            }
//                                            break;
                                        //Reverser
                                        case "Reverser":
                                            throw new NotImplementedException();
//                                            if (ItemToDisplay.ToString() == "Reverser (REV)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "REV ", Value obtain following the colon from the getdata.txt
//                                                //plus % sign and enough spaces to make the text length 16 plus a colon
//                                                msg = ("REV " + splitter[1] + "%").PadRight(padding) + ":";
//                                            }
//                                            break;
                                        //Check Train brake type
                                        case "VirtualBrake":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Train Brake (TB)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "TB ", Value obtain following the colon from the getdata.txt
//                                                //plus % sign and enough spaces to make the text length 16 plus a colon
//                                                msg = ("TB " + splitter[1] + "%").PadRight(padding) + ":";
//                                                BrakeType = "VirtualBrake";
//                                            }
//                                            break;
                                        case "TrainBrakeControl":
                                            throw new NotImplementedException();
//                                            //Because a lot of engines have a VirtualBrake and a Train Brake we need to check we haven't used the virtualbrake or we will get 2 entries for the train brake
//                                            if(BrakeType != "VirtualBrake" && ItemToDisplay.ToString() == "Train Brake (TB)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "TB ", Value obtain following the colon from the getdata.txt
//                                                //plus % sign and enough spaces to make the text length 16 plus a colon
//                                                msg = ("TB " + splitter[1] + "%").PadRight(padding) + ":";
//                                            }
//                                            break;
                                        //Loco Brake
                                        case "EngineBrakeControl":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Loco Brake (LB)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "LB ", Value obtain following the colon from the getdata.txt
//                                                //plus % sign and enough spaces to make the text length 16 plus a colon
//                                                msg = ("LB " + splitter[1] + "%").PadRight(padding) + ":";
//                                            }
//                                            break;

                                        //Dynamic Brake
                                        case "DynamicBrake":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Dynamic Brake (DB)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "DB ", Value obtain following the colon from the getdata.txt
//                                                //plus % sign and enough spaces to make the text length 16 plus a colon
//                                                msg = ("DB " + splitter[1] + "%").PadRight(padding) + ":";
//                                            }
//                                            break;

                                        //Emergency Brake
                                        case "EmergencyBrake":
                                            if(ItemToDisplay.ToString() == "Emergency Brake (EB)")
                                            {
                                                tcp.SendBoolAsInt((splitter[1] != "0"), 2667);
                                            }
                                            break;

                                        //Ammeter
                                        case "Ammeter":
                                            if(ItemToDisplay.ToString() == "Ammeter (Amp)")
                                            {
                                                float value;
                                                if (!float.TryParse(splitter[1], out value))
                                                    continue;
                                                tcp.SendSingle(value, 2567);
                                            }
                                            break;

                                        //RPM
                                        case "RPM":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "RPM (RPM)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "RPM ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("RPM  " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;

                                        //AWS
                                        case "AWS":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "AWS (AWS)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "AWS ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("AWS " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;

                                        //AWS Count
                                        case "AWSWarnCount":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "AWS Count (AWC")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "AWC ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("AWC " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;

                                        //Current & next speed limits
                                        case "Current Speed Limit":
                                            if(ItemToDisplay.ToString() == "Current Speed Limit (CSL)")
                                            {
                                                float value;
                                                if (!float.TryParse(splitter[1], out value))
                                                    continue;
                                                tcp.SendSingle(value, 2660);
                                            }
                                            break;
                                        case "Next Speed Limit":
                                            if(ItemToDisplay.ToString() == "Next Speed Limit (NSL)")
                                            {
                                                float value;
                                                if (!float.TryParse(splitter[1], out value))
                                                    continue;
                                                tcp.SendSingle(value, 2661);
                                            }
                                            break;
                                        case "Next Speed Limit Distance":
                                            if(ItemToDisplay.ToString() == "Next Speed Limit Distance (NSLD)")
                                            {
                                                float value;
                                                if (!float.TryParse(splitter[1], out value))
                                                    continue;
                                                tcp.SendSingle(value, 2662);
                                            }
                                            break;

                                        //Signals
                                        case "Signal Type":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Next Signal Type (NST)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "NST ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("NST " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;

                                        case "Signal State":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Next Signal State (NSS)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "NSS ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("NSS " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;

                                        case "Signal Distance":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Next Signal Distance (NSD)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "NSD ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("NSD " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;

                                        case "Signal Aspect":
                                            throw new NotImplementedException();
//                                            if(ItemToDisplay.ToString() == "Next Signal Aspect (NSA)")
//                                            {
//                                                //Update lines variable, we can only display 4 lines on GLCD
//                                                lines++;
//                                                //Format the text to send to the serial port in the format "NSA ", Value obtain following the colon from the getdata.txt
//                                                //plus enough spaces to make the text length 16 plus a colon
//                                                msg = ("NSA " + splitter[1]).PadRight(padding) + ":";
//                                            }
//                                            break;
                                    }
                                }

                                Thread.Sleep(10);//Sleep for 10 ms
                                Application.DoEvents(); //Allow the computer to perform pending messages in queue
                            }
                            //If we want to send the data to the textbox on the form
                            if (SendToScreen)
                            {
                                //Update msg1
                                msg1 += splitter[0] + ": " + splitter[1] + "\n";
                            }
                            //Has the alerter been activated in game
                            if (splitter[0] == "AWSWarnCount" && Convert.ToInt16(splitter[1]) != 0) //Alerter activated
                            {
                                if (chkPlayAlertSound.Checked)//Do we wish to play the alerter when activated
                                {
                                    bplaysound = true;//Yes so update variable
                                }
                            }
                            else if (splitter[0] == "AWSWarnCount")//AWS Warn count == 0 so do not play sound
                            {
                                bplaysound = false;
                            }
                        }
                    } //while

//                    lines = 0; //Reset lines to 0
                    richTextBox1.Text = msg1; //Update textbox on form with data
                    msg1 = "";
                    Application.DoEvents(); //Allow the computer to perform pending messages in queue
                    Thread.Sleep(10);
                    sr.Close(); //Close streamreader
                    fs.Close(); //Close Filestream
                }
                else //Getdata.txt file does not exist so waiting for Railworks to create it through the Railworks_GetDataScript.
                {
                    label1.Text = "Waiting for Railworks";
                    Application.DoEvents(); //Update textbox on form with data 
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true; //Enable start button
            btnStop.Enabled = false; //Disable stop button
            cboComPort.Enabled = true; //Re enable com port selection
            running = false; //No longer running
            label1.Text = "Status: Stopped";
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            running = false; //No longer running
            SaveConfiguration(); //Save configuration data
            if (sr != null) sr.Close(); //If not closed, close streamreader
            if (fs != null) fs.Close(); //If not closed, close filestraem
        }

        private void chkSendToPort_CheckedChanged(object sender, EventArgs e)
        {
            //Update variable based on check state of sentoport checkbox
            if (chkSendToPort.Checked)
            {
                SendToTcp = true;
            }
            else
            {
                SendToTcp = false;
            }
        }

        private void chkSendToScreen_CheckedChanged(object sender, EventArgs e)
        {
            //Update variable based on check state of sentoscreen checkbox
            if (chkSendToScreen.Checked)
            {
                SendToScreen = true;
            }
            else
            {
                SendToScreen = false;
            }
        }

        private void cboComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            //If com port selected is changed via com port drop down box
            if (cboComPort.SelectedItem.ToString() != comPort)
            {
                //If the selcted com port in selection box does not equal original selected com port then update it.
                comPort = cboComPort.SelectedItem.ToString();
                chkSendToPort.Enabled = true;
            }
        }

        private void clbItemsToDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            //As each item is selcted in items to display in GLCD make sure no more than 4 items are selected
            if (clbItemsToDisplay.CheckedIndices.Count >= 5)
            {
                MessageBox.Show("You can only select 4 items\n"
                    + "Please uncheck an item before selecting another");
                clbItemsToDisplay.SetItemChecked(clbItemsToDisplay.SelectedIndex, false);
            }
        }

        private void SaveConfiguration()
        {
            //Save selected com port and items to display on GLCD data for loading on startup.
            if (comPort != string.Empty)
            {
                StreamWriter sw = new StreamWriter(appPath + "\\Railworks_GetData_Settings.txt");
                sw.WriteLine(comPort);

                foreach (object si in clbItemsToDisplay.CheckedItems)
                {
                    sw.WriteLine(si.ToString());
                }
                sw.Close();
            }
        }

        private void LoadConfiguration()
        {
            //Check to see if we have a Railworks_GetData_Settings.txt file which has the comport to use saved
            if (File.Exists(appPath + "\\Railworks_GetData_Settings.txt"))
            {
                //We have so open it for reading in a streamreader
                sr = new StreamReader(appPath + "\\Railworks_GetData_Settings.txt");
                comPort = sr.ReadLine(); //1st line read from file is the last com port used
                //All other lines if any hold the list of items that were checked in the items to display on GLCD checkedlistbox
                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine(); //s = Speed (Speed), or Throttle (TH), or Reverser (REV), or Train Brake (TB) etc
                    int i = clbItemsToDisplay.FindString(s,0); //Find the index of the item with said string
                    clbItemsToDisplay.SetItemChecked(i,true); //Tick the box
                }
                sr.Close();

                //Check that the comport read from Railworks_GetData_Settings.txt is attached to the computer
                if (cboComPort.Items.Contains(comPort))
                {
                    //It is update the Com Port drop down box
                    cboComPort.SelectedText = comPort;
                    chkSendToPort.Enabled = true; //Tick the send output to com port box
                }
                else
                {
                    //It isn't so inform the user
                    MessageBox.Show("The Serial Port " + comPort + " cannot be found\nPlease plug it in and restart the application\nor select a new port",
                        "Serial port ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    chkSendToPort.Enabled = false; //Make sure send output to com port box is unchecked
                    comPort = string.Empty; //Make com port variable point to nothing
                }
            } //We don't have a Railworks_GetData_Settings.txt file yet so inform the user that they need to select a com port in order to send data to the com port
            else if (cboComPort.Items.Count > 0)
            {
                MessageBox.Show("If you wish to send data to the serial port\n" +
                    "Please select a serial port to use from the dropdown box", "No Serial Port selected");
            }//No com ports attached to the computer so disable the send output to com port checkbox
            else if (comPort == string.Empty)
            {
                chkSendToPort.Enabled = false;
            }

            //Setup sound player
            player = new System.Media.SoundPlayer(AlertSound);
        }
    }
}
