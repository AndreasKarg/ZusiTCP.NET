using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZusiTcpInterface.Zusi3.TypeDescriptors;

namespace ZusiTcpInterfaceTests.Zusi3
{
  [TestClass]
  public class CabInfoDescriptorTests
  {
    [TestMethod]
    public void Equal_descriptors_are_considered_equal()
    {
      // Given
      var attributesA = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var attributesB = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var nodesA = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var nodesB = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var a = new CabInfoNodeDescriptor(0, "Root", attributesA, nodesA);
      var b = new CabInfoNodeDescriptor(0, "Root", attributesB, nodesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsTrue(areEqual);
      Assert.IsFalse(areInequal);
      Assert.IsTrue(explicitEquals);
      Assert.IsTrue(objectEquals);
    }

    [TestMethod]
    public void Top_level_inequality_treated_as_inequal()
    {
      // Given
      var attributesA = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/sx", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var attributesB = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var nodesA = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var nodesB = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var a = new CabInfoNodeDescriptor(0, "Root", attributesA, nodesA);
      var b = new CabInfoNodeDescriptor(0, "Root", attributesB, nodesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsFalse(areEqual);
      Assert.IsTrue(areInequal);
      Assert.IsFalse(explicitEquals);
      Assert.IsFalse(objectEquals);
    }

    [TestMethod]
    public void Nested_inequality_treated_as_inequal()
    {
      // Given
      var attributesA = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var attributesB = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var nodesA = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "baromatiksauce", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var nodesB = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var a = new CabInfoNodeDescriptor(0, "Root", attributesA, nodesA);
      var b = new CabInfoNodeDescriptor(0, "Root", attributesB, nodesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsFalse(areEqual);
      Assert.IsTrue(areInequal);
      Assert.IsFalse(explicitEquals);
      Assert.IsFalse(objectEquals);
    }

    [TestMethod]
    public void Different_number_of_attributes_is_inequal()
    {
      // Given
      var attributesA = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var attributesB = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
      };

      var nodesA = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var nodesB = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var a = new CabInfoNodeDescriptor(0, "Root", attributesA, nodesA);
      var b = new CabInfoNodeDescriptor(0, "Root", attributesB, nodesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsFalse(areEqual);
      Assert.IsTrue(areInequal);
      Assert.IsFalse(explicitEquals);
      Assert.IsFalse(objectEquals);
    }

    [TestMethod]
    public void Different_number_of_nodes_is_inequal()
    {
      // Given
      var attributesA = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var attributesB = new[]
      {
        new CabInfoAttributeDescriptor(0x01, "Geschwindigkeit", "m/s", "Single"),
        new CabInfoAttributeDescriptor(0x02, "Druck Hauptluftleitung", "bar", "Single"),
      };

      var nodesA = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala")
      };

      var nodesB = new[]
      { new CabInfoNodeDescriptor(0x123, "Test", new []
        {
          new CabInfoAttributeDescriptor(0x03, "Druck Bremszylinder", "bar", "Single"),
          new CabInfoAttributeDescriptor(0x04, "Druck Hauptluftbehälter", "bar", "Single", "Mit Sauce"),
          new CabInfoAttributeDescriptor(0x05, "Luftpresser läuft", "aus/an", "BoolAsSingle"),
        }, "Lalala"),
        new CabInfoNodeDescriptor(0x153, "Test2", new []
        {
          new CabInfoAttributeDescriptor(0x06, "Luftstrom Fbv", "-1...0...1", "Fail"),
          new CabInfoAttributeDescriptor(0x07, "Luftstrom Zbv", "-1...0...1", "Single"),
          new CabInfoAttributeDescriptor(0x08, "Lüfter an", "aus/an", "BoolAsSingle")
        })};

      var a = new CabInfoNodeDescriptor(0, "Root", attributesA, nodesA);
      var b = new CabInfoNodeDescriptor(0, "Root", attributesB, nodesB);

      // When
      var areEqual = a == b;
      var areInequal = a != b;
      var explicitEquals = a.Equals(b);
      var objectEquals = a.Equals((object)b);

      // Then
      Assert.IsFalse(areEqual);
      Assert.IsTrue(areInequal);
      Assert.IsFalse(explicitEquals);
      Assert.IsFalse(objectEquals);
    }
  }
}