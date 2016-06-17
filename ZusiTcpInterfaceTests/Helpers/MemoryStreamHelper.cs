using System.IO;

namespace ZusiTcpInterfaceTests.Helpers
{
  public static class MemoryStreamHelpers
  {
    public static void ReinitialiseWith(this MemoryStream stream, byte[] newData)
    {
      stream.SetLength(0);
      stream.Write(newData, 0, newData.Length);
      stream.Seek(0, SeekOrigin.Begin);
    }
  }
}