using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface;

namespace ZusiTcpInterfaceTests
{
  [TestClass]
  public class AddressTests
  {
    [TestMethod]
    public void Identical_addresses_are_treated_as_equal()
    {
      // Given
      var address1 = new Address(1, 2, 3);
      var address2 = new Address(1, 2, 3);

      // When
      var areEqual = address1 == address2;
      var areInequal = address1 != address2;

      // Then
      Assert.IsTrue(areEqual);
      Assert.IsFalse(areInequal);
      Assert.AreEqual(address1, address2);
      Assert.IsTrue(address1.Equals(address2));
    }

    [TestMethod]
    public void CabInfoAddress_is_equal_to_normal_address_with_CabInfo_node_IDs_prefixed()
    {
      // Given
      short cabDataId = 0x17;
      var address1 = new CabInfoAddress(cabDataId);
      var address2 = new Address(0x02, 0x0A, cabDataId);

      // When
      var areEqual = address1 == address2;
      var areInequal = address1 != address2;

      // Then
      Assert.IsTrue(areEqual);
      Assert.IsFalse(areInequal);
      Assert.AreEqual(address1, address2);
      Assert.IsTrue(address1.Equals(address2));
    }
  }
}