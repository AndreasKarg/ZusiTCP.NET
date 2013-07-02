using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Zusi_Datenausgabe
{
  public interface INetworkIOHandler : IBinaryReader
  {
    void SendToServer(byte[] message);
    void SendPacket(params byte[] message);
    void SendPacket(params byte[][] message);
    void Close();
    void Disconnect();
  }

  public class NetworkIOHandler : INetworkIOHandler, IDisposable
  {
    private readonly TcpClient _clientConnection = new TcpClient(AddressFamily.InterNetwork);
    private NetworkStream _clientStream;

    private BinaryReader _clientReader;

    public NetworkIOHandler(IPEndPoint endPoint)
    {
      EstablishConnection(endPoint);
    }

    public void SendToServer(byte[] message)
    {
      try
      {
        _clientStream.Write(message, 0, message.Length);
      }
      catch (IOException ex)
      {
        throw new ZusiTcpException("An error occured when trying to send data to the server.", ex);
      }
    }

    public void SendPacket(params byte[] message)
    {
      SendToServer(BitConverter.GetBytes(message.Length));
      SendToServer(message);
    }

    public void SendPacket(params byte[][] message)
    {
      int iTempLength = message.Sum(item => item.Length);

      SendToServer(BitConverter.GetBytes(iTempLength));

      foreach (var item in message)
      {
        SendToServer(item);
      }
    }

    private void EstablishConnection(IPEndPoint endPoint)
    {
      try
      {
        _clientConnection.Connect(endPoint);
      }
      catch (SocketException ex)
      {
        throw new ZusiTcpException("Could not establish socket connection to TCP server. " +
                                   "Is the server running and enabled?",
          ex);
      }

      Debug.Assert(_clientConnection.Connected);

      _clientStream = _clientConnection.GetStream();
      _clientReader = new BinaryReader(_clientStream);
    }

    #region IBinaryReader delegating members
    public int PeekChar()
    {
      return _clientReader.PeekChar();
    }

    public int Read()
    {
      return _clientReader.Read();
    }

    public bool ReadBoolean()
    {
      return _clientReader.ReadBoolean();
    }

    public byte ReadByte()
    {
      return _clientReader.ReadByte();
    }

    public sbyte ReadSByte()
    {
      return _clientReader.ReadSByte();
    }

    public char ReadChar()
    {
      return _clientReader.ReadChar();
    }

    public short ReadInt16()
    {
      return _clientReader.ReadInt16();
    }

    public ushort ReadUInt16()
    {
      return _clientReader.ReadUInt16();
    }

    public int ReadInt32()
    {
      return _clientReader.ReadInt32();
    }

    public uint ReadUInt32()
    {
      return _clientReader.ReadUInt32();
    }

    public long ReadInt64()
    {
      return _clientReader.ReadInt64();
    }

    public ulong ReadUInt64()
    {
      return _clientReader.ReadUInt64();
    }

    public float ReadSingle()
    {
      return _clientReader.ReadSingle();
    }

    public double ReadDouble()
    {
      return _clientReader.ReadDouble();
    }

    public decimal ReadDecimal()
    {
      return _clientReader.ReadDecimal();
    }

    public string ReadString()
    {
      return _clientReader.ReadString();
    }

    public int Read(char[] buffer, int index, int count)
    {
      return _clientReader.Read(buffer, index, count);
    }

    public char[] ReadChars(int count)
    {
      return _clientReader.ReadChars(count);
    }

    public int Read(byte[] buffer, int index, int count)
    {
      return _clientReader.Read(buffer, index, count);
    }

    public byte[] ReadBytes(int count)
    {
      return _clientReader.ReadBytes(count);
    }
    #endregion

    #region Implementation of IDisposable and related methods

    public void Dispose()
    {
      Disconnect();
    }

    public void Close()
    {
      Disconnect();
    }

    public void Disconnect()
    {
      _clientConnection.Close();
      _clientReader.Close();
      _clientStream.Close();
    }

    #endregion
  }
}