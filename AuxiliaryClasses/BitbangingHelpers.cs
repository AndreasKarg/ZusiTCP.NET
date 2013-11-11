using System.Diagnostics.Contracts;

namespace Zusi_Datenausgabe.AuxiliaryClasses
{
  public static class BitbangingHelpers
  {
    [Pure]
    public static byte[] Pack(params byte[] message)
    {
      return message;
    }

    [Pure]
    public static int GetInstruction(int byteA, int byteB)
    {
      return byteA * 256 + byteB;
    }
  }
}