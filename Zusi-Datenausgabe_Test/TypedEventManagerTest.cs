using System.Diagnostics;
using Castle.Windsor;
using Zusi_Datenausgabe;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Zusi_Datenausgabe_Test
{


  /// <summary>
  ///This is a test class for TypedEventManagerTest and is intended
  ///to contain all TypedEventManagerTest Unit Tests
  ///</summary>
  [TestClass]
  public class TypedEventManagerTest
  {


    private TestContext _testContextInstance;
    private IWindsorContainer _container;
    private ITypedEventManager _target;
    private int _eventHasBeenCalled;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return _testContextInstance;
      }
      set
      {
        _testContextInstance = value;
      }
    }

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test

    [TestInitialize]
    public void MyTestInitialize()
    {
      _container = BootstrapContainer();
      _target = _container.Resolve<ITypedEventManager>();
      _eventHasBeenCalled = 0;
    }
    //
    //Use TestCleanup to run code after each test has run
    [TestCleanup]
    public void MyTestCleanup()
    {
      _container.Dispose();
      _container = null;
    }
    //
    #endregion

    private IWindsorContainer BootstrapContainer()
    {
      return new WindsorContainer()
        .Install(new WindsorInstaller()
        );
    }

    [TestMethod]
    public void InvokeTest()
    {
      _target.Subscribe<bool>(BoolEventHandler);
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, true));
      Assert.IsTrue(_eventHasBeenCalled == 1);
    }

    [TestMethod]
    public void SubscribeTest()
    {
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, true));
      Assert.IsTrue(_eventHasBeenCalled == 0);

      _target.Subscribe<bool>(BoolEventHandler);
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, true));
      Assert.IsTrue(_eventHasBeenCalled == 1);

      _target.Subscribe<bool>(BoolEventHandler);
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, true));
      Assert.IsTrue(_eventHasBeenCalled == 2);
    }

    private void GenericEventHandler<T>(object sender, DataReceivedEventArgs<T> data)
    {
      _eventHasBeenCalled++;
      Trace.WriteLine("Event handler for type {0} has been called.", typeof(T).ToString());
    }

    private void BoolEventHandler(object sender, DataReceivedEventArgs<bool> shouldHaveBeenCalled)
    {
      Trace.WriteLine("Bool event handler has been called. It should {0}have been.", (shouldHaveBeenCalled.Value) ? "" : "*not* ");
      _eventHasBeenCalled++;
      Assert.IsTrue(shouldHaveBeenCalled.Value);
    }

    [TestMethod]
    public void UnsubscribeTest()
    {
      _target.Subscribe<bool>(BoolEventHandler);
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, true));
      Assert.IsTrue(_eventHasBeenCalled == 1);
      _target.Unsubscribe<bool>(BoolEventHandler);
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, false));
      Assert.IsTrue(_eventHasBeenCalled == 1);

      _target.Unsubscribe<bool>(BoolEventHandler);
    }
  }
}
