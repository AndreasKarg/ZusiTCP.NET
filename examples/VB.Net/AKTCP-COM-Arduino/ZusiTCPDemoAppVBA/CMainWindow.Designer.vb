<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CMainWindow
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.TbServer = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.groupBox1 = New System.Windows.Forms.GroupBox()
        Me.BtnConnect = New System.Windows.Forms.Button()
        Me.TbPort = New System.Windows.Forms.TextBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.Serial = New System.IO.Ports.SerialPort(Me.components)
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.LabelHW = New System.Windows.Forms.Label()
        Me.ComCon = New System.Windows.Forms.Button()
        Me.CbCom = New System.Windows.Forms.ComboBox()
        Me.groupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TbServer
        '
        Me.TbServer.Location = New System.Drawing.Point(6, 38)
        Me.TbServer.Name = "TbServer"
        Me.TbServer.Size = New System.Drawing.Size(130, 20)
        Me.TbServer.TabIndex = 1
        Me.TbServer.Text = "localhost"
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(6, 19)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(126, 13)
        Me.label1.TabIndex = 3
        Me.label1.Text = "Server IP oder Hostname"
        '
        'groupBox1
        '
        Me.groupBox1.Controls.Add(Me.BtnConnect)
        Me.groupBox1.Controls.Add(Me.TbPort)
        Me.groupBox1.Controls.Add(Me.label2)
        Me.groupBox1.Controls.Add(Me.TbServer)
        Me.groupBox1.Controls.Add(Me.label1)
        Me.groupBox1.Location = New System.Drawing.Point(12, 12)
        Me.groupBox1.Name = "groupBox1"
        Me.groupBox1.Size = New System.Drawing.Size(142, 136)
        Me.groupBox1.TabIndex = 4
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "Zusi"
        '
        'BtnConnect
        '
        Me.BtnConnect.Location = New System.Drawing.Point(9, 107)
        Me.BtnConnect.Name = "BtnConnect"
        Me.BtnConnect.Size = New System.Drawing.Size(127, 22)
        Me.BtnConnect.TabIndex = 6
        Me.BtnConnect.Text = "Verbinden"
        Me.BtnConnect.UseVisualStyleBackColor = True
        '
        'TbPort
        '
        Me.TbPort.Location = New System.Drawing.Point(6, 80)
        Me.TbPort.Name = "TbPort"
        Me.TbPort.Size = New System.Drawing.Size(130, 20)
        Me.TbPort.TabIndex = 4
        Me.TbPort.Text = "1435"
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(6, 61)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(26, 13)
        Me.label2.TabIndex = 5
        Me.label2.Text = "Port"
        '
        'Serial
        '
        Me.Serial.PortName = "COM7"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.LabelHW)
        Me.GroupBox2.Controls.Add(Me.ComCon)
        Me.GroupBox2.Controls.Add(Me.CbCom)
        Me.GroupBox2.Location = New System.Drawing.Point(160, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(142, 136)
        Me.GroupBox2.TabIndex = 10
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Hardware"
        '
        'LabelHW
        '
        Me.LabelHW.AutoSize = True
        Me.LabelHW.Location = New System.Drawing.Point(2, 19)
        Me.LabelHW.Name = "LabelHW"
        Me.LabelHW.Size = New System.Drawing.Size(134, 13)
        Me.LabelHW.TabIndex = 3
        Me.LabelHW.Text = "Fahrpult angeschlossen an"
        '
        'ComCon
        '
        Me.ComCon.Location = New System.Drawing.Point(5, 106)
        Me.ComCon.Name = "ComCon"
        Me.ComCon.Size = New System.Drawing.Size(131, 23)
        Me.ComCon.TabIndex = 1
        Me.ComCon.Text = "Verbinden"
        Me.ComCon.UseVisualStyleBackColor = True
        '
        'CbCom
        '
        Me.CbCom.FormattingEnabled = True
        Me.CbCom.Location = New System.Drawing.Point(5, 38)
        Me.CbCom.Name = "CbCom"
        Me.CbCom.Size = New System.Drawing.Size(131, 21)
        Me.CbCom.TabIndex = 0
        '
        'CMainWindow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(312, 165)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.groupBox1)
        Me.Name = "CMainWindow"
        Me.Text = "Zusi > TCP > COM > Arduino"
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents TbServer As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents BtnConnect As System.Windows.Forms.Button
    Private WithEvents TbPort As System.Windows.Forms.TextBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Friend WithEvents Serial As System.IO.Ports.SerialPort
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents CbCom As System.Windows.Forms.ComboBox
    Friend WithEvents ComCon As System.Windows.Forms.Button
    Friend WithEvents LabelHW As System.Windows.Forms.Label

End Class
