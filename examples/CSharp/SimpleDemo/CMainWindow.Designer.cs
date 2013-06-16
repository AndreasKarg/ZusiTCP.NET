namespace ZusiTCPDemoApp
{
    partial class CMainWindow
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
                MyTCPConnection.Dispose();
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
            this.TbServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnConnect = new System.Windows.Forms.Button();
            this.TbPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            //this.SpeedLabel = new System.Windows.Forms.Label();
            //this.TELabel = new System.Windows.Forms.Label();

			this.lblGeschw = new System.Windows.Forms.Label();
			this.lblFahrstufe = new System.Windows.Forms.Label();
			this.lblUhrzeit = new System.Windows.Forms.Label();
			this.lblPZB = new System.Windows.Forms.Label();
			this.lblFreiBis = new System.Windows.Forms.Label();
			this.lblLZB = new System.Windows.Forms.Label();
			this.RichTextBox1 = new System.Windows.Forms.RichTextBox();

			this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TbServer
            // 
            this.TbServer.Location = new System.Drawing.Point(6, 38);
            this.TbServer.Name = "TbServer";
            this.TbServer.Size = new System.Drawing.Size(100, 20);
            this.TbServer.TabIndex = 1;
            this.TbServer.Text = "localhost";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server address";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BtnConnect);
            this.groupBox1.Controls.Add(this.TbPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TbServer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(112, 136);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Control";
            // 
            // BtnConnect
            // 
            this.BtnConnect.Location = new System.Drawing.Point(9, 107);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Size = new System.Drawing.Size(97, 22);
            this.BtnConnect.TabIndex = 6;
            this.BtnConnect.Text = "Connect";
            this.BtnConnect.UseVisualStyleBackColor = true;
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // TbPort
            // 
            this.TbPort.Location = new System.Drawing.Point(6, 80);
            this.TbPort.Name = "TbPort";
            this.TbPort.Size = new System.Drawing.Size(100, 20);
            this.TbPort.TabIndex = 4;
            this.TbPort.Text = "1435";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            /*// 
            // SpeedLabel
            // 
            this.SpeedLabel.AutoSize = true;
            this.SpeedLabel.Location = new System.Drawing.Point(131, 30);
            this.SpeedLabel.Name = "SpeedLabel";
            this.SpeedLabel.Size = new System.Drawing.Size(78, 13);
            this.SpeedLabel.TabIndex = 5;
            this.SpeedLabel.Text = "Speed: ? km/h";
            // 
            // TELabel
            // 
            this.TELabel.AutoSize = true;
            this.TELabel.Location = new System.Drawing.Point(134, 47);
            this.TELabel.Name = "TELabel";
            this.TELabel.Size = new System.Drawing.Size(134, 13);
            this.TELabel.TabIndex = 6;
            this.TELabel.Text = "Overall traction effort: ? kN";*/










			//        
			//lblGeschw
			//
			this.lblGeschw.AutoSize = true;
			this.lblGeschw.Location = new System.Drawing.Point(159, 12);
			this.lblGeschw.Name = "lblGeschw";
			this.lblGeschw.Size = new System.Drawing.Size(125, 13);
			this.lblGeschw.TabIndex = 6;
			this.lblGeschw.Text = "Geschwindigkeit: ? km/h";
			//
			//lblFahrstufe
			//
			this.lblFahrstufe.AutoSize = true;
			this.lblFahrstufe.Location = new System.Drawing.Point(194, 25);
			this.lblFahrstufe.Name = "lblFahrstufe";
			this.lblFahrstufe.Size = new System.Drawing.Size(63, 13);
			this.lblFahrstufe.TabIndex = 6;
			this.lblFahrstufe.Text = "Fahrstufe: ?";
			//
			//lblUhrzeit
			//
			this.lblUhrzeit.AutoSize = true; 
			this.lblUhrzeit.Location = new System.Drawing.Point(205, 38);
			this.lblUhrzeit.Name = "lblUhrzeit";
			this.lblUhrzeit.Size = new System.Drawing.Size(52, 13);
			this.lblUhrzeit.TabIndex = 6;
			this.lblUhrzeit.Text = "Uhrzeit: ?";
			//
			//lblPZB
			//
			this.lblPZB.AutoSize = true;
			this.lblPZB.Location = new System.Drawing.Point(217, 51);
			this.lblPZB.Name = "lblPZB";
			this.lblPZB.Size = new System.Drawing.Size(40, 13);
			this.lblPZB.TabIndex = 6;
			this.lblPZB.Text = "PZB: ?";
			//
			//lblFreiBis
			//
			this.lblFreiBis.AutoSize = true;
			this.lblFreiBis.Location = new System.Drawing.Point(181, 95);
			this.lblFreiBis.Name = "lblFreiBis";
			this.lblFreiBis.Size = new System.Drawing.Size(52, 13);
			this.lblFreiBis.TabIndex = 6;
			this.lblFreiBis.Text = "Frei bis: ?";
			//
			//lblLZB
			//
			this.lblLZB.AutoSize = true;
			this.lblLZB.Location = new System.Drawing.Point(218, 64);
			this.lblLZB.Name = "lblLZB";
			this.lblLZB.Size = new System.Drawing.Size(39, 13);
			this.lblLZB.TabIndex = 7;
			this.lblLZB.Text = "LZB: ?";
			//
			//RichTextBox1
			//
			this.RichTextBox1.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top 
			                                                               | System.Windows.Forms.AnchorStyles.Bottom) 
			                                                               | System.Windows.Forms.AnchorStyles.Left) 
			                                                               | System.Windows.Forms.AnchorStyles.Right);
			this.RichTextBox1.EnableAutoDragDrop = true;
			this.RichTextBox1.Location = new System.Drawing.Point(12, 154);
			this.RichTextBox1.Name = "RichTextBox1";
			this.RichTextBox1.ReadOnly = true;
			this.RichTextBox1.Size = new System.Drawing.Size(469, 282);
			this.RichTextBox1.TabIndex = 8;
			this.RichTextBox1.Text = "";
			this.RichTextBox1.ZoomFactor = 0.75F;




            // 
            // CMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(493, 448);
            //this.Controls.Add(this.TELabel);
            //this.Controls.Add(this.SpeedLabel);
			this.Controls.Add(this.RichTextBox1);
			this.Controls.Add(this.lblLZB);
			this.Controls.Add(this.lblFreiBis);
			this.Controls.Add(this.lblPZB);
			this.Controls.Add(this.lblUhrzeit);
			this.Controls.Add(this.lblFahrstufe);
			this.Controls.Add(this.lblGeschw);


			this.Controls.Add(this.groupBox1);
			this.Name = "CMainWindow";
			this.Text = "Zusi TCP Demo 1 - Receive and Show Datas - C#";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TbServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnConnect;
        private System.Windows.Forms.TextBox TbPort;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblGeschw;
		private System.Windows.Forms.Label lblFahrstufe;
		private System.Windows.Forms.Label lblUhrzeit;
		private System.Windows.Forms.Label lblPZB;
		private System.Windows.Forms.Label lblFreiBis;
		private System.Windows.Forms.Label lblLZB;
		private System.Windows.Forms.RichTextBox RichTextBox1;
	}
}

