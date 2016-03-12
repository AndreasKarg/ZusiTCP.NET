namespace ZusiTcpInterface.Common
{
  internal interface IReadableStream
  {
    byte[] Read(int i);
    byte[] Peek(int i);
  }
}