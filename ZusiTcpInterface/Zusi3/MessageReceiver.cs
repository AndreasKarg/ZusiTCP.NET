using System;
using System.Collections.Generic;
using System.IO;
using ZusiTcpInterface.Zusi3.Converters;
using ZusiTcpInterface.Zusi3.DOM;

namespace ZusiTcpInterface.Zusi3
{
  internal class MessageReceiver : IMessageReceiver
  {
    private readonly BinaryReader _binaryReader;
    private readonly RootNodeConverter _rootNodeConverter;
    private readonly IEnumerator<IProtocolChunk> _chunkEnumerable;

    public MessageReceiver(BinaryReader binaryReader, RootNodeConverter rootNodeConverter)
    {
      _binaryReader = binaryReader;
      _rootNodeConverter = rootNodeConverter;
      _chunkEnumerable = GetChunkEnumerable().GetEnumerator();
    }

    private IEnumerable<IProtocolChunk> GetChunkEnumerable()
    {
      while (true)
      {
        var message = Node.Deserialise(_binaryReader);
        var chunks = _rootNodeConverter.Convert(message);

        foreach (var chunk in chunks)
        {
          yield return chunk;
        }
      }
    }

    public IProtocolChunk GetNextChunk()
    {
      if(!_chunkEnumerable.MoveNext())
        throw new InvalidOperationException("Can't get next chunk: Connection must have terminated.");
      return _chunkEnumerable.Current;
    }
  }
}