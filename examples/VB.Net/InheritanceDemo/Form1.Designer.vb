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
        Me.lblOut = New System.Windows.Forms.Label
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
        Me.groupBox1.TabIndex = 6
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
        'lblOut
        '
        Me.lblOut.AutoSize = True
        Me.lblOut.Font = New System.Drawing.Font("Courier New", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOut.Location = New System.Drawing.Point(213, 9)
        Me.lblOut.Name = "lblOut"
        Me.lblOut.Size = New System.Drawing.Size(0, 14)
        Me.lblOut.TabIndex = 7
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(460, 287)
        Me.Controls.Add(Me.lblOut)
        Me.Controls.Add(Me.groupBox1)
        Me.Name = "Form1"
        Me.Text = "Zusi TCP Demo 2 - Create a new Datatype - VBA"
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
    Friend WithEvents lblOut As System.Windows.Forms.Label

End Class
