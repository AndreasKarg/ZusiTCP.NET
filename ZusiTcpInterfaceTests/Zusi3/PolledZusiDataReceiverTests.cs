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
    private readonly Queue<DataChunkBase> _cabDataChunks = new Queue<DataChunkBase>();
    private readonly PolledZusiDataReceiver _polledZusiDataReceiver;
    private readonly AttributeDescriptor _floatDescriptor = new AttributeDescriptor(1, "Float", "N/A", "N/A");
    private readonly AttributeDescriptor _boolDescriptor = new AttributeDescriptor(2, "Bool", "N/A", "N/A");

    public PolledZusiDataReceiverTests()
    {
      var mockQueue = new Mock<IBlockingCollection<DataChunkBase>>();
      mockQueue.Setup(mock => mock.Take())
        .Returns(() => _cabDataChunks.Dequeue());

      mockQueue.Setup(mock => mock.Count)
        .Returns(() => _cabDataChunks.Count);

      var descriptors = new List<AttributeDescriptor>
      {
        _floatDescriptor,
        _boolDescriptor
      };

      var descriptorCollection = new NodeDescriptor(0, "Root", descriptors);

      _polledZusiDataReceiver = new PolledZusiDataReceiver(mockQueue.Object, descriptorCollection);
    }

    [TestMethod]
    public void Raises_correct_events_when_supplied_with_data_chunks()
    {
      // Given
      float? lastReceivedFloat = null;
      Address lastReceivedFloatId = null;
      string lastReceivedFloatName = null;

      bool? lastReceivedBool = null;
      Address lastReceivedBoolId = null;
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
      var floatAddress = new Address(_floatDescriptor.Id);
      var boolAddress = new Address(_boolDescriptor.Id);

      _cabDataChunks.Enqueue(new DataChunk<float>(floatAddress, expectedFloat));
      _cabDataChunks.Enqueue(new DataChunk<bool>(boolAddress, expectedBool));

      // When
      _polledZusiDataReceiver.Service();

      // Then
      Assert.AreEqual(expectedFloat, lastReceivedFloat);
      Assert.AreEqual(floatAddress, lastReceivedFloatId);
      Assert.AreEqual(_floatDescriptor.Name, lastReceivedFloatName);

      Assert.AreEqual(expectedBool, lastReceivedBool);
      Assert.AreEqual(boolAddress, lastReceivedBoolId);
      Assert.AreEqual(_boolDescriptor.Name, lastReceivedBoolName);
    }

    [TestMethod]
    public void Throws_exception_when_receiving_unknown_cab_data_chunk_type()
    {
      // Given
      _cabDataChunks.Enqueue(new DataChunk<PolledZusiDataReceiverTests>(new Address(1233), null));

      // When/Then
      Assert.Throws<NotSupportedException>(_polledZusiDataReceiver.Service);
    }
  }
}