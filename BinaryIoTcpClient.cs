using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Zusi_Datenausgabe
{
  public interface IBinaryIO : IBinaryReader
  {
    void SendToPeer(byte[] message);
    bool Connected { get; }
  }

  internal class BinaryIoTcpClient : IBinaryIO
  {
    private NetworkStream _clientStream;
    private BinaryReader _clientReader;
    private readonly TcpClient _tcpClient;
    private bool _isDisposed;
    private bool _haveOwnershipOfTcpClient;

    public BinaryIoTcpClient(TcpClient tcpClient, bool takeOwnership)
    {
      _tcpClient = tcpClient;
      InitializeStreamReader();
      _haveOwnershipOfTcpClient = takeOwnership;
    }

    public static BinaryIoTcpClient CreateConnection(string hostname, int port)
    {
      return new BinaryIoTcpClient(new TcpClient(hostname, port), true);
    }

    public static BinaryIoTcpClient CreateConnection(IPAddress address, int port)
    {
      var client = new TcpClient();
      client.Connect(address, port);
      return new BinaryIoTcpClient(client, true);
    }

    public static BinaryIoTcpClient CreateConnection(IPEndPoint remoteEP)
    {
      return new BinaryIoTcpClient(new TcpClient(remoteEP), true);
    }

    public bool Connected
    {
      get { return _tcpClient.Connected; }
    }

    private void InitializeStreamReader()
    {
      Debug.Assert(_tcpClient.Connected);

      _clientStream = _tcpClient.GetStream();

      //TODO: Cleanup
      _clientReader = new BinaryReader(_clientStream, new ASCIIEncoding());
    }

    public void Close()
    {
      if (_haveOwnershipOfTcpClient)
      {
        _clientReader.Close();
        _clientStream.Close();
        _tcpClient.Close();
      }

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

    public void SendToPeer(byte[] message)
    {
      ValidateConnection();
      _clientStream.Write(message, 0, message.Length);
    }

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