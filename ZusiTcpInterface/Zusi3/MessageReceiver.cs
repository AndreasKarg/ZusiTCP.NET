using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ZusiTcpInterface.Zusi3
{
  internal class MessageReceiver
  {
    private readonly BinaryReader _binaryReader;
    private readonly TopLevelNodeConverter _rootNodeConverter;
    private readonly IBlockingCollection<IProtocolChunk> _blockingChunkQueue;

    public MessageReceiver(BinaryReader binaryReader, TopLevelNodeConverter rootNodeConverter, IBlockingCollection<IProtocolChunk> blockingChunkQueue)
    {
      _binaryReader = binaryReader;
      _rootNodeConverter = rootNodeConverter;
      _blockingChunkQueue = blockingChunkQueue;
    }

    public void ProcessNextPacket()
    {
      var message = Node.Deserialise(_binaryReader);
      var chunks = _rootNodeConverter.Convert(message);

      foreach (var chunk in chunks)
      {
        _blockingChunkQueue.Add(chunk);
      }
    }
  }
}