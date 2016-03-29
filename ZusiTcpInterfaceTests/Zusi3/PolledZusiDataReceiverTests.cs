using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class PolledZusiDataReceiverTests
  {
    private readonly Queue<CabDataChunkBase> _cabDataChunks = new Queue<CabDataChunkBase>();
    private readonly PolledZusiDataReceiver _polledZusiDataReceiver;

    public PolledZusiDataReceiverTests()
    {
      var mockQueue = new Mock<IBlockingCollection<CabDataChunkBase>>();
      mockQueue.Setup(mock => mock.Take())
        .Returns(() => _cabDataChunks.Dequeue());

      mockQueue.Setup(mock => mock.Count)
        .Returns(() => _cabDataChunks.Count);

      _polledZusiDataReceiver = new PolledZusiDataReceiver(mockQueue.Object);
    }

    [TestMethod]
    public void Raises_correct_events_when_supplied_with_data_chunks()
    {
      // Given
      float? lastReceivedFloat = null;
      bool? lastReceivedBool = null;

      _polledZusiDataReceiver.BoolReceived += (sender, args) => { lastReceivedBool = args.Payload; };
      _polledZusiDataReceiver.FloatReceived += (sender, args) => { lastReceivedFloat = args.Payload; };

      var expectedFloat = 3.0f;
      var expectedBool = true;

      _cabDataChunks.Enqueue(new CabDataChunk<float>(1, expectedFloat));
      _cabDataChunks.Enqueue(new CabDataChunk<bool>(2, expectedBool));

      // When
      _polledZusiDataReceiver.Service();

      // Then
      Assert.AreEqual(expectedFloat, lastReceivedFloat);
      Assert.AreEqual(expectedBool, lastReceivedBool);
    }
  }
}