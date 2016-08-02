using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ZusiTcpInterface;
using ZusiTcpInterface.TypeDescriptors;

namespace ZusiTcpInterfaceTests
{
  [TestClass]
  public class NeededDataCollectionTests
  {
    [TestMethod]
    public void Requested_data_types_are_available_as_addresses()
    {
      // Given
      var geschwindigkeitDescriptor = new AttributeDescriptor(new CabInfoAddress(0x01), "Geschwindigkeit", "Geschwindigkeit", "ja", "nein", "weiss nicht" );
      var sifaDescriptor = new AttributeDescriptor(new CabInfoAddress(0x05), "LM Sifa", "LM Sifa", "Daniel", "Duesentrieb");
      var deDescriptor = new AttributeDescriptor(new CabInfoAddress(0xDE), "DE", "DE", "DE", "DE");
      var adbeDescriptor = new AttributeDescriptor(new CabInfoAddress(0xAD, 0xBE), "ADBE", "ADBE", "ADBE", "EF");

      var descriptorCollection = new DescriptorCollection(new []
      {
        geschwindigkeitDescriptor,
        new AttributeDescriptor(new CabInfoAddress(0x12), "Toastbrot", "Toastbrot", "Marmelade", "Im Schuh"),
        sifaDescriptor,
        new AttributeDescriptor(new CabInfoAddress(0x06), "Dosenwurst", "Dosenwurst", "Wurst", "In Der Dose"),
        deDescriptor,
        adbeDescriptor,
      });

      var neededDataCollection = new NeededDataCollection(descriptorCollection);

      // When
      neededDataCollection.Request("Geschwindigkeit", "LM Sifa");
      neededDataCollection.Request(new CabInfoAddress(0xDE), new CabInfoAddress(0xAD, 0xBE));

      var requestedValuesAsDescriptors = neededDataCollection.GetRequestedDescriptors().ToArray();
      var requestedValuesAsAddresses = neededDataCollection.GetRequestedAddresses().ToArray();

      // Then
      Assert.AreEqual(4, requestedValuesAsDescriptors.Length);
      Assert.AreEqual(4, requestedValuesAsAddresses.Length);

      Assert.IsTrue(requestedValuesAsDescriptors.Contains(geschwindigkeitDescriptor));
      Assert.IsTrue(requestedValuesAsAddresses.Contains(geschwindigkeitDescriptor.Address));

      Assert.IsTrue(requestedValuesAsDescriptors.Contains(sifaDescriptor));
      Assert.IsTrue(requestedValuesAsAddresses.Contains(sifaDescriptor.Address));

      Assert.IsTrue(requestedValuesAsDescriptors.Contains(deDescriptor));
      Assert.IsTrue(requestedValuesAsAddresses.Contains(deDescriptor.Address));

      Assert.IsTrue(requestedValuesAsDescriptors.Contains(adbeDescriptor));
      Assert.IsTrue(requestedValuesAsAddresses.Contains(adbeDescriptor.Address));
    }
  }
}