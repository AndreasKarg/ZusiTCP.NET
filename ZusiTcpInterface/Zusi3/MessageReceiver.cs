using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace ZusiTcpInterface.Zusi3
{
  internal class MessageReceiver
  {
    private readonly BinaryReader _binaryReader;
    private readonly TopLevelNodeConverter _rootNodeConverter;
    private readonly BlockingCollection<IProtocolChunk> _blockingChunkQueue;

    public MessageReceiver(BinaryReader binaryReader, TopLevelNodeConverter rootNodeConverter, BlockingCollection<IProtocolChunk> blockingChunkQueue)
    {
      _binaryReader = binaryReader;
      _rootNodeConverter = rootNodeConverter;
      _blockingChunkQueue = blockingChunkQueue;
    }

    public void StartReceptionLoop()
    {
      Task.Run((Action)ReceptionLoop);
    }

    private void ReceptionLoop()
    {
      // ToDo: Add cancellation token
      while (true)
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
}