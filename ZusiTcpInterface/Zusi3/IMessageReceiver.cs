namespace ZusiTcpInterface.Zusi3
{
  internal interface IMessageReceiver
  {
    IProtocolChunk GetNextChunk();
  }
}