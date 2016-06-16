namespace WinFormsDemoApp
{
  partial class MainWindow
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
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.lblGearboxIndicator = new System.Windows.Forms.Label();
      this.lblVelocity = new System.Windows.Forms.Label();
      this.lblConnecting = new System.Windows.Forms.Label();
      this.lblSifaStatus = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(30, 44);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(47, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Velocity:";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(30, 57);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(93, 13);
      this.label3.TabIndex = 1;
      this.label3.Text = "Gearbox indicator:";
      // 
      // lblGearboxIndicator
      // 
      this.lblGearboxIndicator.AutoSize = true;
      this.lblGearboxIndicator.Location = new System.Drawing.Point(129, 57);
      this.lblGearboxIndicator.Name = "lblGearboxIndicator";
      this.lblGearboxIndicator.Size = new System.Drawing.Size(89, 13);
      this.lblGearboxIndicator.TabIndex = 2;
      this.lblGearboxIndicator.Text = "No data received";
      // 
      // lblVelocity
      // 
      this.lblVelocity.AutoSize = true;
      this.lblVelocity.Location = new System.Drawing.Point(129, 44);
      this.lblVelocity.Name = "lblVelocity";
      this.lblVelocity.Size = new System.Drawing.Size(89, 13);
      this.lblVelocity.TabIndex = 3;
      this.lblVelocity.Text = "No data received";
      // 
      // lblConnecting
      // 
      this.lblConnecting.AutoSize = true;
      this.lblConnecting.Location = new System.Drawing.Point(33, 193);
      this.lblConnecting.Name = "lblConnecting";
      this.lblConnecting.Size = new System.Drawing.Size(70, 13);
      this.lblConnecting.TabIndex = 4;
      this.lblConnecting.Text = "Connecting...";
      // 
      // lblSifaStatus
      // 
      this.lblSifaStatus.AutoSize = true;
      this.lblSifaStatus.Location = new System.Drawing.Point(129, 70);
      this.lblSifaStatus.Name = "lblSifaStatus";
      this.lblSifaStatus.Size = new System.Drawing.Size(89, 13);
      this.lblSifaStatus.TabIndex = 6;
      this.lblSifaStatus.Text = "No data received";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(30, 70);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(59, 13);
      this.label5.TabIndex = 5;
      this.label5.Text = "Sifa status:";
      // 
      // MainWindow
      // 
      this.ClientSize = new System.Drawing.Size(429, 261);
      this.Controls.Add(this.lblSifaStatus);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.lblConnecting);
      this.Controls.Add(this.lblVelocity);
      this.Controls.Add(this.lblGearboxIndicator);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Name = "MainWindow";
      this.Text = "WinForms Demo App";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
      this.Load += new System.EventHandler(this.MainWindow_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblGearboxIndicator;
    private System.Windows.Forms.Label lblVelocity;
    private System.Windows.Forms.Label lblConnecting;
    private System.Windows.Forms.Label lblSifaStatus;
    private System.Windows.Forms.Label label5;
  }
}

