using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Zusi_Datenausgabe
{
  internal class BinaryIoTcpClient : IBinaryReader
  {
    private NetworkStream _clientStream;
    private BinaryReader _clientReader;
    private readonly TcpClient _tcpClient = new TcpClient();
    private bool _isDisposed;

    public void Connect(string hostname, int port)
    {
      ValidateIsNotDisposed();
      _tcpClient.Connect(hostname, port);
      InitializeStreamReader();
    }

    public void Connect(IPAddress address, int port)
    {
      ValidateIsNotDisposed();
      _tcpClient.Connect(address, port);
      InitializeStreamReader();
    }

    public void Connect(IPEndPoint remoteEP)
    {
      ValidateIsNotDisposed();
      _tcpClient.Connect(remoteEP);
      InitializeStreamReader();
    }

    public bool Connected
    {
      get { return _tcpClient.Connected; }
    }

    private void InitializeStreamReader()
    {
      ValidateConnection();
      Debug.Assert(_tcpClient.Connected);

      _clientStream = _tcpClient.GetStream();
      _clientReader = new BinaryReader(_clientStream);
    }

    public void Close()
    {
      _clientReader.Close();
      _clientStream.Close();
      _tcpClient.Close();
      _isDisposed = true;
    }

    public void Dispose()
    {
      Close();
    }

    private void ValidateConnection()
    {
      ValidateIsNotDisposed();
      if (!Connected)
        throw new ZusiTcpException("Tried to access an unconnected Tcp connection.");
      Debug.Assert(_clientReader != null, "_clientReader == null");
    }

    private void ValidateIsNotDisposed()
    {
      if(_isDisposed) throw new ObjectDisposedException("This object has been disposed and cannot be used anymore.");
    }

    #region Delegating members.

    public int PeekChar()
    {
      ValidateConnection();
      return _clientReader.PeekChar();
    }

    public int Read(byte[] buffer, int index, int count)
    {
      ValidateConnection();
      return _clientReader.Read(buffer, index, count);
    }

    public int Read(char[] buffer, int index, int count)
    {
      ValidateConnection();
      return _clientReader.Read(buffer, index, count);
    }

    public int Read()
    {
      ValidateConnection();
      return _clientReader.Read();
    }

    public bool ReadBoolean()
    {
      ValidateConnection();
      return _clientReader.ReadBoolean();
    }

    public byte ReadByte()
    {
      ValidateConnection();
      return _clientReader.ReadByte();
    }

    public byte[] ReadBytes(int count)
    {
      ValidateConnection();
      return _clientReader.ReadBytes(count);
    }

    public char ReadChar()
    {
      ValidateConnection();
      return _clientReader.ReadChar();
    }

    public char[] ReadChars(int count)
    {
      ValidateConnection();
      return _clientReader.ReadChars(count);
    }

    public decimal ReadDecimal()
    {
      ValidateConnection();
      return _clientReader.ReadDecimal();
    }

    public double ReadDouble()
    {
      ValidateConnection();
      return _clientReader.ReadDouble();
    }

    public short ReadInt16()
    {
      ValidateConnection();
      return _clientReader.ReadInt16();
    }

    public int ReadInt32()
    {
      ValidateConnection();
      return _clientReader.ReadInt32();
    }

    public long ReadInt64()
    {
      ValidateConnection();
      return _clientReader.ReadInt64();
    }

    public sbyte ReadSByte()
    {
      ValidateConnection();
      return _clientReader.ReadSByte();
    }

    public float ReadSingle()
    {
      ValidateConnection();
      return _clientReader.ReadSingle();
    }

    public string ReadString()
    {
      ValidateConnection();
      return _clientReader.ReadString();
    }

    public ushort ReadUInt16()
    {
      ValidateConnection();
      return _clientReader.ReadUInt16();
    }

    public uint ReadUInt32()
    {
      ValidateConnection();
      return _clientReader.ReadUInt32();
    }

    public ulong ReadUInt64()
    {
      ValidateConnection();
      return _clientReader.ReadUInt64();
    }
    #endregion
  }
}