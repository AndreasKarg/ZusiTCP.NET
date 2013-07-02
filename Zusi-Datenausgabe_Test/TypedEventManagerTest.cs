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
    private bool _eventHasBeenCalled;

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

    /// <summary>
    ///A test for CastToListType
    ///</summary>
    public void CastToListTypeTestHelper<T>()
    {
      PrivateObject param0 = null; // TODO: Initialize to an appropriate value
      TypedEventManager_Accessor target = new TypedEventManager_Accessor(param0); // TODO: Initialize to an appropriate value
      ITypedMethodListBase handlerList = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> expected = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> actual;
      actual = target.CastToListType<T>(handlerList);
      Assert.AreEqual(expected, actual);
      Assert.Inconclusive("Verify the correctness of this test method.");
    }

    [TestMethod]
    [DeploymentItem("Zusi-Datenausgabe.dll")]
    public void CastToListTypeTest()
    {
      CastToListTypeTestHelper<GenericParameterHelper>();
    }

    /// <summary>
    ///A test for CreateNewList
    ///</summary>
    public void CreateNewListTestHelper<T>()
    {
      PrivateObject param0 = null; // TODO: Initialize to an appropriate value
      TypedEventManager_Accessor target = new TypedEventManager_Accessor(param0); // TODO: Initialize to an appropriate value
      Type type = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> expected = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> actual;
      actual = target.CreateNewList<T>(type);
      Assert.AreEqual(expected, actual);
      Assert.Inconclusive("Verify the correctness of this test method.");
    }

    [TestMethod]
    [DeploymentItem("Zusi-Datenausgabe.dll")]
    public void CreateNewListTest()
    {
      CreateNewListTestHelper<GenericParameterHelper>();
    }

    /// <summary>
    ///A test for GetDictionaryEntry
    ///</summary>
    public void GetDictionaryEntryTestHelper<T>()
    {
      PrivateObject param0 = null; // TODO: Initialize to an appropriate value
      TypedEventManager_Accessor target = new TypedEventManager_Accessor(param0); // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> expected = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> actual;
      actual = target.GetDictionaryEntry<T>();
      Assert.AreEqual(expected, actual);
      Assert.Inconclusive("Verify the correctness of this test method.");
    }

    [TestMethod]
    [DeploymentItem("Zusi-Datenausgabe.dll")]
    public void GetDictionaryEntryTest()
    {
      GetDictionaryEntryTestHelper<GenericParameterHelper>();
    }

    /// <summary>
    ///A test for Invoke
    ///</summary>
    public void InvokeTestHelper<T>()
    {
      IDictionary<Type, ITypedMethodListBase> eventHandlers = null; // TODO: Initialize to an appropriate value
      ITypedMethodListFactory methodListFactory = null; // TODO: Initialize to an appropriate value
      TypedEventManager target = new TypedEventManager(eventHandlers, methodListFactory); // TODO: Initialize to an appropriate value
      object sender = null; // TODO: Initialize to an appropriate value
      DataReceivedEventArgs<T> eventArgs = null; // TODO: Initialize to an appropriate value
      target.Invoke<T>(sender, eventArgs);
      Assert.Inconclusive("A method that does not return a value cannot be verified.");
    }

    [TestMethod]
    public void InvokeTest()
    {
      InvokeTestHelper<GenericParameterHelper>();
    }

    /// <summary>
    ///A test for Subscribe
    ///</summary>
    public void SubscribeTestHelper<T>()
    {
      IDictionary<Type, ITypedMethodListBase> eventHandlers = null; // TODO: Initialize to an appropriate value
      ITypedMethodListFactory methodListFactory = null; // TODO: Initialize to an appropriate value
      TypedEventManager target = new TypedEventManager(eventHandlers, methodListFactory); // TODO: Initialize to an appropriate value
      EventHandler<DataReceivedEventArgs<T>> handler = null; // TODO: Initialize to an appropriate value
      target.Subscribe<T>(handler);
      Assert.Inconclusive("A method that does not return a value cannot be verified.");
    }

    [TestMethod]
    public void SubscribeTest()
    {
      SubscribeTestHelper<GenericParameterHelper>();
    }

    /// <summary>
    ///A test for TryGetDictionaryEntry
    ///</summary>
    public void TryGetDictionaryEntryTestHelper<T>()
    {
      PrivateObject param0 = null; // TODO: Initialize to an appropriate value
      TypedEventManager_Accessor target = new TypedEventManager_Accessor(param0); // TODO: Initialize to an appropriate value
      Type type = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> result = null; // TODO: Initialize to an appropriate value
      ITypedMethodList<DataReceivedEventArgs<T>> resultExpected = null; // TODO: Initialize to an appropriate value
      bool expected = false; // TODO: Initialize to an appropriate value
      bool actual;
      actual = target.TryGetDictionaryEntry<T>(type, out result);
      Assert.AreEqual(resultExpected, result);
      Assert.AreEqual(expected, actual);
      Assert.Inconclusive("Verify the correctness of this test method.");
    }

    [TestMethod]
    [DeploymentItem("Zusi-Datenausgabe.dll")]
    public void TryGetDictionaryEntryTest()
    {
      TryGetDictionaryEntryTestHelper<GenericParameterHelper>();
    }

    private void GenericEventHandler<T>(object sender, DataReceivedEventArgs<T> data)
    {
      _eventHasBeenCalled = true;
      Trace.WriteLine("Event handler for type {0} has been called.", typeof(T).ToString());
    }

    private void BoolEventHandler(object sender, DataReceivedEventArgs<bool> shouldHaveBeenCalled)
    {
      Trace.WriteLine("Bool event handler has been called. It should {0}have been.", (shouldHaveBeenCalled.Value) ? "" : "*not* ");
      _eventHasBeenCalled = true;
      Assert.IsTrue(shouldHaveBeenCalled.Value);
    }

    [TestMethod]
    public void UnsubscribeTest()
    {
      _target.Subscribe<bool>(BoolEventHandler);
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, true));
      Assert.IsTrue(_eventHasBeenCalled);
      _target.Unsubscribe<bool>(BoolEventHandler);

      _eventHasBeenCalled = false;
      _target.Invoke(this, new DataReceivedEventArgs<bool>(123, false));
      Assert.IsFalse(_eventHasBeenCalled);
    }
  }
}
