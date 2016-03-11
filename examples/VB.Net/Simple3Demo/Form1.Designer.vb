<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.groupBox1 = New System.Windows.Forms.GroupBox
        Me.BtnConnect = New System.Windows.Forms.Button
        Me.TbPort = New System.Windows.Forms.TextBox
        Me.label2 = New System.Windows.Forms.Label
        Me.TbServer = New System.Windows.Forms.TextBox
        Me.label1 = New System.Windows.Forms.Label
        Me.lblGeschw = New System.Windows.Forms.Label
        Me.lblFahrstufe = New System.Windows.Forms.Label
        Me.lblUhrzeit = New System.Windows.Forms.Label
        Me.lblPZB = New System.Windows.Forms.Label
        Me.lblFreiBis = New System.Windows.Forms.Label
        Me.lblLZB = New System.Windows.Forms.Label
        Me.BtnJump = New System.Windows.Forms.Button
        Me.BtnSpeed = New System.Windows.Forms.Button
        Me.BtnTexture = New System.Windows.Forms.Button
        Me.BtnAP = New System.Windows.Forms.Button
        Me.BtnPause = New System.Windows.Forms.Button
        Me.groupBox1.SuspendLayout()
        Me.SuspendLayout()
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
        Me.groupBox1.Size = New System.Drawing.Size(112, 136)
        Me.groupBox1.TabIndex = 5
        Me.groupBox1.TabStop = False
        Me.groupBox1.Text = "Control"
        '
        'BtnConnect
        '
        Me.BtnConnect.Location = New System.Drawing.Point(9, 107)
        Me.BtnConnect.Name = "BtnConnect"
        Me.BtnConnect.Size = New System.Drawing.Size(97, 22)
        Me.BtnConnect.TabIndex = 6
        Me.BtnConnect.Text = "Connect"
        Me.BtnConnect.UseVisualStyleBackColor = True
        '
        'TbPort
        '
        Me.TbPort.Location = New System.Drawing.Point(6, 80)
        Me.TbPort.Name = "TbPort"
        Me.TbPort.Size = New System.Drawing.Size(100, 20)
        Me.TbPort.TabIndex = 4
        Me.TbPort.Text = "1436"
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
        'TbServer
        '
        Me.TbServer.Location = New System.Drawing.Point(6, 38)
        Me.TbServer.Name = "TbServer"
        Me.TbServer.Size = New System.Drawing.Size(100, 20)
        Me.TbServer.TabIndex = 1
        Me.TbServer.Text = "localhost"
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(6, 19)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(78, 13)
        Me.label1.TabIndex = 3
        Me.label1.Text = "Server address"
        '
        'lblGeschw
        '
        Me.lblGeschw.AutoSize = True
        Me.lblGeschw.Location = New System.Drawing.Point(159, 12)
        Me.lblGeschw.Name = "lblGeschw"
        Me.lblGeschw.Size = New System.Drawing.Size(125, 13)
        Me.lblGeschw.TabIndex = 6
        Me.lblGeschw.Text = "Geschwindigkeit: ? km/h"
        '
        'lblFahrstufe
        '
        Me.lblFahrstufe.AutoSize = True
        Me.lblFahrstufe.Location = New System.Drawing.Point(194, 25)
        Me.lblFahrstufe.Name = "lblFahrstufe"
        Me.lblFahrstufe.Size = New System.Drawing.Size(63, 13)
        Me.lblFahrstufe.TabIndex = 6
        Me.lblFahrstufe.Text = "Fahrstufe: ?"
        '
        'lblUhrzeit
        '
        Me.lblUhrzeit.AutoSize = True
        Me.lblUhrzeit.Location = New System.Drawing.Point(205, 38)
        Me.lblUhrzeit.Name = "lblUhrzeit"
        Me.lblUhrzeit.Size = New System.Drawing.Size(52, 13)
        Me.lblUhrzeit.TabIndex = 6
        Me.lblUhrzeit.Text = "Uhrzeit: ?"
        '
        'lblPZB
        '
        Me.lblPZB.AutoSize = True
        Me.lblPZB.Location = New System.Drawing.Point(217, 51)
        Me.lblPZB.Name = "lblPZB"
        Me.lblPZB.Size = New System.Drawing.Size(40, 13)
        Me.lblPZB.TabIndex = 6
        Me.lblPZB.Text = "PZB: ?"
        '
        'lblFreiBis
        '
        Me.lblFreiBis.AutoSize = True
        Me.lblFreiBis.Location = New System.Drawing.Point(181, 95)
        Me.lblFreiBis.Name = "lblFreiBis"
        Me.lblFreiBis.Size = New System.Drawing.Size(52, 13)
        Me.lblFreiBis.TabIndex = 6
        Me.lblFreiBis.Text = "Frei bis: ?"
        '
        'lblLZB
        '
        Me.lblLZB.AutoSize = True
        Me.lblLZB.Location = New System.Drawing.Point(218, 64)
        Me.lblLZB.Name = "lblLZB"
        Me.lblLZB.Size = New System.Drawing.Size(39, 13)
        Me.lblLZB.TabIndex = 7
        Me.lblLZB.Text = "LZB: ?"
        '
        'BtnJump
        '
        Me.BtnJump.Location = New System.Drawing.Point(395, 41)
        Me.BtnJump.Name = "BtnJump"
        Me.BtnJump.Size = New System.Drawing.Size(23, 23)
        Me.BtnJump.TabIndex = 16
        Me.BtnJump.Text = ">|"
        Me.BtnJump.UseVisualStyleBackColor = True
        '
        'BtnSpeed
        '
        Me.BtnSpeed.Location = New System.Drawing.Point(356, 41)
        Me.BtnSpeed.Name = "BtnSpeed"
        Me.BtnSpeed.Size = New System.Drawing.Size(33, 23)
        Me.BtnSpeed.TabIndex = 17
        Me.BtnSpeed.Text = ">>"
        Me.BtnSpeed.UseVisualStyleBackColor = True
        '
        'BtnTexture
        '
        Me.BtnTexture.Location = New System.Drawing.Point(356, 99)
        Me.BtnTexture.Name = "BtnTexture"
        Me.BtnTexture.Size = New System.Drawing.Size(62, 46)
        Me.BtnTexture.TabIndex = 15
        Me.BtnTexture.Text = "Texture" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(ICE 1)"
        Me.BtnTexture.UseVisualStyleBackColor = True
        '
        'BtnAP
        '
        Me.BtnAP.Location = New System.Drawing.Point(356, 70)
        Me.BtnAP.Name = "BtnAP"
        Me.BtnAP.Size = New System.Drawing.Size(62, 23)
        Me.BtnAP.TabIndex = 13
        Me.BtnAP.Text = "AutoPilot"
        Me.BtnAP.UseVisualStyleBackColor = True
        '
        'BtnPause
        '
        Me.BtnPause.Location = New System.Drawing.Point(356, 12)
        Me.BtnPause.Name = "BtnPause"
        Me.BtnPause.Size = New System.Drawing.Size(62, 23)
        Me.BtnPause.TabIndex = 14
        Me.BtnPause.Text = "Pause"
        Me.BtnPause.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(493, 183)
        Me.Controls.Add(Me.BtnJump)
        Me.Controls.Add(Me.BtnSpeed)
        Me.Controls.Add(Me.BtnTexture)
        Me.Controls.Add(Me.BtnAP)
        Me.Controls.Add(Me.BtnPause)
        Me.Controls.Add(Me.lblLZB)
        Me.Controls.Add(Me.lblFreiBis)
        Me.Controls.Add(Me.lblPZB)
        Me.Controls.Add(Me.lblUhrzeit)
        Me.Controls.Add(Me.lblFahrstufe)
        Me.Controls.Add(Me.lblGeschw)
        Me.Controls.Add(Me.groupBox1)
        Me.Name = "Form1"
        Me.Text = "Zusi TCP Demo 1 - Receive and Show Datas - VBA"
        Me.groupBox1.ResumeLayout(False)
        Me.groupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents groupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents BtnConnect As System.Windows.Forms.Button
    Private WithEvents TbPort As System.Windows.Forms.TextBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents TbServer As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Friend WithEvents lblGeschw As System.Windows.Forms.Label
    Friend WithEvents lblFahrstufe As System.Windows.Forms.Label
    Friend WithEvents lblUhrzeit As System.Windows.Forms.Label
    Friend WithEvents lblPZB As System.Windows.Forms.Label
    Friend WithEvents lblFreiBis As System.Windows.Forms.Label
    Friend WithEvents lblLZB As System.Windows.Forms.Label
    Friend WithEvents BtnJump As System.Windows.Forms.Button
    Friend WithEvents BtnSpeed As System.Windows.Forms.Button
    Friend WithEvents BtnTexture As System.Windows.Forms.Button
    Friend WithEvents BtnAP As System.Windows.Forms.Button
    Friend WithEvents BtnPause As System.Windows.Forms.Button

End Class
