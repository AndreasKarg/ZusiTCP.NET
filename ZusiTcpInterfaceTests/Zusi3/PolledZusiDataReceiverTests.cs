using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSTestExtensions;
using System;
using System.Collections.Generic;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class PolledZusiDataReceiverTests : BaseTest
  {
    private readonly Queue<CabDataChunkBase> _cabDataChunks = new Queue<CabDataChunkBase>();
    private readonly PolledZusiDataReceiver _polledZusiDataReceiver;
    private readonly CabInfoAttributeDescriptor _floatDescriptor = new CabInfoAttributeDescriptor(1, "Float", "N/A", "N/A");
    private readonly CabInfoAttributeDescriptor _boolDescriptor = new CabInfoAttributeDescriptor(2, "Bool", "N/A", "N/A");

    public PolledZusiDataReceiverTests()
    {
      var mockQueue = new Mock<IBlockingCollection<CabDataChunkBase>>();
      mockQueue.Setup(mock => mock.Take())
        .Returns(() => _cabDataChunks.Dequeue());

      mockQueue.Setup(mock => mock.Count)
        .Returns(() => _cabDataChunks.Count);

      var descriptors = new List<CabInfoAttributeDescriptor>
      {
        _floatDescriptor,
        _boolDescriptor
      };

      var descriptorCollection = new CabInfoNodeDescriptor(0, "Root", descriptors);

      _polledZusiDataReceiver = new PolledZusiDataReceiver(mockQueue.Object, descriptorCollection);
    }

    [TestMethod]
    public void Raises_correct_events_when_supplied_with_data_chunks()
    {
      // Given
      float? lastReceivedFloat = null;
      short? lastReceivedFloatId = null;
      string lastReceivedFloatName = null;

      bool? lastReceivedBool = null;
      short? lastReceivedBoolId = null;
      string lastReceivedBoolName = null;

      _polledZusiDataReceiver.BoolReceived += (sender, args) =>
      {
        lastReceivedBool = args.Payload;
        lastReceivedBoolId = args.Id;
        lastReceivedBoolName = args.Descriptor.Name;
      };

      _polledZusiDataReceiver.FloatReceived += (sender, args) =>
      {
        lastReceivedFloat = args.Payload;
        lastReceivedFloatId = args.Id;
        lastReceivedFloatName = args.Descriptor.Name;
      };

      const float expectedFloat = 3.0f;
      const bool expectedBool = true;

      _cabDataChunks.Enqueue(new CabDataChunk<float>(_floatDescriptor.Id, expectedFloat));
      _cabDataChunks.Enqueue(new CabDataChunk<bool>(_boolDescriptor.Id, expectedBool));

      // When
      _polledZusiDataReceiver.Service();

      // Then
      Assert.AreEqual(expectedFloat, lastReceivedFloat);
      Assert.AreEqual(_floatDescriptor.Id, lastReceivedFloatId);
      Assert.AreEqual(_floatDescriptor.Name, lastReceivedFloatName);

      Assert.AreEqual(expectedBool, lastReceivedBool);
      Assert.AreEqual(_boolDescriptor.Id, lastReceivedBoolId);
      Assert.AreEqual(_boolDescriptor.Name, lastReceivedBoolName);
    }

    [TestMethod]
    public void Throws_exception_when_receiving_unknown_cab_data_chunk_type()
    {
      // Given
      _cabDataChunks.Enqueue(new CabDataChunk<PolledZusiDataReceiverTests>(1233, null));

      // When/Then
      Assert.Throws<NotSupportedException>(_polledZusiDataReceiver.Service);
    }
  }
}