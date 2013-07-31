using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zusi_Datenausgabe_Test.AuxiliaryClasses
{
  [TestClass]
  public class SimpleSynchronizationContextTest
  {
    private SimpleSynchronizationContext _context;
    private int _eventCalled;

    [TestInitialize]
    public void InitializeTest()
    {
      _context = new SimpleSynchronizationContext();
      _eventCalled = 0;
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestSend()
    {
      // Should crash because unsupported.
      _context.Send(TestEvent, true);
    }

    [TestMethod]
    public void TestPostWithOneThread()
    {
      const int numTests = 10;

      _context.Post(TestEvent, false);

      Assert.IsTrue(_eventCalled == 0, "Event called prematurely.");

      _context.HandleOne();

      Assert.IsTrue(_eventCalled == 1);

      for (int i = 0; i < numTests; i++)
      {
        _context.Post(TestEvent, false);
      }

      _eventCalled = 0;
      _context.HandleAll();
      Assert.IsTrue(_eventCalled == numTests);
    }

    [TestMethod]
    public void TestPostWithSeparateThread()
    {
      RunTestsOnSeparateThreads(1, 10, false);
    }

    [TestMethod]
    public void TestPostWithMultipleThreads()
    {
      RunTestsOnSeparateThreads(10, 10, false);
    }

    private void RunTestsOnSeparateThreads(int numThreads, int numEventCallsPerThread, bool failEvent)
    {
      var tasks = new List<Task>();

      var threadParams = new SimpleSynchronizationContextTest.ThreadParams(numEventCallsPerThread, failEvent);

      for (int i = 0; i < numThreads; i++)
      {
        var task = new Task(ThreadMethod, threadParams);
        tasks.Add(task);
        task.Start();
      }

      Task.WaitAll(tasks.ToArray());

      Assert.IsTrue(_eventCalled == 0);

      _context.HandleAll();
      Assert.IsTrue(_eventCalled == numThreads*numEventCallsPerThread);
    }

    private void ThreadMethod(object par)
    {
      Assert.IsNotNull(par);
      var pars = (ThreadParams)par;

      for (int i = 0; i < pars.NumEventCalls; i++)
      {
        _context.Post(TestEvent, pars.FailEvent);
      }
    }

    private void TestEvent(object state)
    {
      bool fail = (bool) state;
      _eventCalled++;
      Assert.IsFalse(fail, "Event should not have been called.");
    }

    private struct ThreadParams
    {
      public int NumEventCalls { get; private set; }
      public bool FailEvent { get; private set; }

      public ThreadParams(int numEventCalls, bool failEvent) : this()
      {
        NumEventCalls = numEventCalls;
        FailEvent = failEvent;
      }
    }
  }
}
