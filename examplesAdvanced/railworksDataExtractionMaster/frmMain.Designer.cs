namespace Railworks_GetData
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkSendToPort = new System.Windows.Forms.CheckBox();
            this.chkSendToScreen = new System.Windows.Forms.CheckBox();
            this.cboComPort = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.clbItemsToDisplay = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkPlayAlertSound = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(17, 678);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(163, 678);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(17, 90);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(290, 568);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(17, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Status: Stopped";
            // 
            // chkSendToPort
            // 
            this.chkSendToPort.AutoSize = true;
            this.chkSendToPort.Location = new System.Drawing.Point(17, 67);
            this.chkSendToPort.Name = "chkSendToPort";
            this.chkSendToPort.Size = new System.Drawing.Size(149, 17);
            this.chkSendToPort.TabIndex = 4;
            this.chkSendToPort.Text = "Send output to Comm port";
            this.chkSendToPort.UseVisualStyleBackColor = true;
            this.chkSendToPort.CheckedChanged += new System.EventHandler(this.chkSendToPort_CheckedChanged);
            // 
            // chkSendToScreen
            // 
            this.chkSendToScreen.AutoSize = true;
            this.chkSendToScreen.Checked = true;
            this.chkSendToScreen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSendToScreen.Location = new System.Drawing.Point(17, 44);
            this.chkSendToScreen.Name = "chkSendToScreen";
            this.chkSendToScreen.Size = new System.Drawing.Size(139, 17);
            this.chkSendToScreen.TabIndex = 5;
            this.chkSendToScreen.Text = "Send Output To Screen";
            this.chkSendToScreen.UseVisualStyleBackColor = true;
            this.chkSendToScreen.CheckedChanged += new System.EventHandler(this.chkSendToScreen_CheckedChanged);
            // 
            // cboComPort
            // 
            this.cboComPort.FormattingEnabled = true;
            this.cboComPort.Location = new System.Drawing.Point(239, 12);
            this.cboComPort.Name = "cboComPort";
            this.cboComPort.Size = new System.Drawing.Size(121, 21);
            this.cboComPort.Sorted = true;
            this.cboComPort.TabIndex = 6;
            this.cboComPort.SelectedIndexChanged += new System.EventHandler(this.cboComPort_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(183, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Com Port";
            // 
            // clbItemsToDisplay
            // 
            this.clbItemsToDisplay.CheckOnClick = true;
            this.clbItemsToDisplay.FormattingEnabled = true;
            this.clbItemsToDisplay.Items.AddRange(new object[] {
            "Speed (Speed)",
            "Throttle (TH)",
            "Reverser (REV)",
            "Train Brake (TB)",
            "Loco Brake (LB)",
            "Dynamic Brake (DB)",
            "Emergency Brake (EB)",
            "Ammeter (Amp)",
            "RPM (RPM)",
            "AWS (AWS)",
            "AWS Count (AWC)",
            "Current Speed Limit (CSL)",
            "Next Speed Limit (NSL)",
            "Next Speed Limit Distance (NSLD)",
            "Next Signal Type (NST)",
            "Next Signal State (NSS)",
            "Next Signal Distance (NSD)",
            "Next Signal Aspect (NSA)"});
            this.clbItemsToDisplay.Location = new System.Drawing.Point(324, 85);
            this.clbItemsToDisplay.Name = "clbItemsToDisplay";
            this.clbItemsToDisplay.Size = new System.Drawing.Size(197, 454);
            this.clbItemsToDisplay.TabIndex = 8;
            this.clbItemsToDisplay.SelectedIndexChanged += new System.EventHandler(this.clbItemsToDisplay_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(321, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(194, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Select up to 4 items to display on GLCD";
            // 
            // chkPlayAlertSound
            // 
            this.chkPlayAlertSound.AutoSize = true;
            this.chkPlayAlertSound.Location = new System.Drawing.Point(186, 44);
            this.chkPlayAlertSound.Name = "chkPlayAlertSound";
            this.chkPlayAlertSound.Size = new System.Drawing.Size(113, 17);
            this.chkPlayAlertSound.TabIndex = 10;
            this.chkPlayAlertSound.Text = "Play Alerter Sound";
            this.chkPlayAlertSound.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 720);
            this.Controls.Add(this.chkPlayAlertSound);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.clbItemsToDisplay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboComPort);
            this.Controls.Add(this.chkSendToScreen);
            this.Controls.Add(this.chkSendToPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "frmMain";
            this.Text = "Railworks / TS2014 Data Extractor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkSendToPort;
        private System.Windows.Forms.CheckBox chkSendToScreen;
        private System.Windows.Forms.ComboBox cboComPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox clbItemsToDisplay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkPlayAlertSound;
    }
}

