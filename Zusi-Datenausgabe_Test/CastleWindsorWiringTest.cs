using System.CodeDom.Compiler;
using System.IO;
using Castle.Windsor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zusi_Datenausgabe;

namespace Zusi_Datenausgabe_Test
{
  /// <summary>
  /// Summary description for CastleWindsorWiringTest
  /// </summary>
  [TestClass]
  public class CastleWindsorWiringTest
  {
    public CastleWindsorWiringTest()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    [TestMethod]
    public void TestMethod1()
    {
      IWindsorContainer container = new WindsorContainer()
        .Install(new WindsorInstaller()
        );

      var visualizer = new DependencyGraphWriter(container, new IndentedTextWriter(new StreamWriter(".\\dependency_graph.txt")));

      visualizer.Output();
    }
  }
}
