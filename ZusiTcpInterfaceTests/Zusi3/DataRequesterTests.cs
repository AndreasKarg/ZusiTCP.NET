using System.Collections;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class DataRequesterTests
  {
    [TestMethod]
    public void Contains_correct_addresses_for_requested_data()
    {
      // Given
      var expectedAddresses = new[]
      {
        new Address(0x0A, 0x16),
        new Address(0x0A, 0x64, 0x1C),
      };

      var attributesOnSecondSubLevel = new[]
      {
        new CabInfoAttributeDescriptor(0x1C, "Sub Level Attribute A", "none", "some"),
        new CabInfoAttributeDescriptor(0x1D, "Sub Level Attribute B", "none", "some"),
        new CabInfoAttributeDescriptor(0x1E, "Sub Level Attribute C", "none", "some"),
      };

      var attributesOnRootLevel = new[]
      {
        new CabInfoAttributeDescriptor(0x15, "Root Level Attribute A", "none", "some"),
        new CabInfoAttributeDescriptor(0x16, "Root Level Attribute B", "none", "some"),
        new CabInfoAttributeDescriptor(0x17, "Root Level Attribute C", "none", "some"),
      };

      var secondSubLevelDescriptor = new CabInfoNodeDescriptor(0x03, "Sub Level 2", attributesOnSecondSubLevel);
      var firstSubLevelDescriptor = new CabInfoNodeDescriptor(0x64, "Sub Level 1", Enumerable.Empty<CabInfoAttributeDescriptor>(), new [] { secondSubLevelDescriptor });
      var rootDescriptor = new CabInfoNodeDescriptor(0x0A, "Root", attributesOnRootLevel, new[] {firstSubLevelDescriptor});

      var descriptors = new DescriptorCollection<CabInfoNodeDescriptor>(new []{ rootDescriptor });

      var dataRequester = new DataRequester(descriptors);

      // When
      dataRequester.Request("Root Level Attribute B");
      dataRequester.Request("Sub Level Attribute A");

      // Then
      Assert.AreEqual(expectedAddresses.Length, dataRequester.RequestedAddresses.Count);

      foreach (var address in expectedAddresses)
      {
        Assert.IsTrue(dataRequester.RequestedAddresses.Contains(address));
      }
    }
  }
}
