namespace ZusiTcpInterface
{
  internal interface IMessageReceiver
  {
    IProtocolChunk GetNextChunk();
  }
}