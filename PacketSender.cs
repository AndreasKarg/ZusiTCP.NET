using System;
using System.Linq;

namespace Zusi_Datenausgabe
{
  public class PacketSender
  {
    public IBinaryIO ClientConnection { get; set; }

    public void SendPacket(params byte[] message)
    {
      ClientConnection.SendToPeer(PackArrays(new byte[][] {BitConverter.GetBytes(message.Length), message}));
    }

    public void SendLargePacket(params byte[][] message)
    {
      int iTempLength = message.Sum(item => item.Length);

      ClientConnection.SendToPeer(PackArrays(BitConverter.GetBytes(iTempLength), message));
    }

    public static byte[] Pack(params byte[] message)
    {
      return message;
    }

    public static byte[] PackArrays(byte[] firstPacket, byte[][] otherPackets)
    {
      byte[][] partialPackets = new byte[otherPackets.Length + 1][];
      partialPackets[0] = firstPacket;
      Array.Copy(otherPackets, 0, partialPackets, 1, otherPackets.Length);
      return PackArrays(partialPackets);
    }

    public static byte[] PackArrays(byte[][] partialPackets)
    {
      byte[] value = new byte[partialPackets.Sum(item => item.Length)];
      int curLoc = 0;
      foreach(byte[] arr in partialPackets)
      {
        Array.Copy(arr, 0, value, curLoc, arr.Length);
        curLoc += arr.Length;
      }
      return value;
    }
  }
}