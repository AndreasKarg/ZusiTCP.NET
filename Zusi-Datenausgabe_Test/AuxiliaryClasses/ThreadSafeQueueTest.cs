using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zusi_Datenausgabe_Test.AuxiliaryClasses
{
  [TestClass]
  public class ThreadSafeQueueTest
  {
    [TestMethod]
    public void TestWithOneThread()
    {
      var queue = new ThreadSafeQueue<string>();

      Assert.IsTrue(queue.IsSynchronized);

      string string1 = "String 1";
      string string2 = "String 2";

      queue.Enqueue(string1);
      Assert.IsTrue(queue.Count == 1, "Queue count is not 1.");

      queue.Enqueue(string2);
      Assert.IsTrue(queue.Count == 2, "Queue count is not 2.");

      var dequeued = queue.Dequeue();
      Assert.AreEqual(dequeued, string1);
      Assert.IsTrue(queue.Count == 1);

      dequeued = queue.Peek();
      Assert.AreEqual(dequeued, string2);
      Assert.IsTrue(queue.Count == 1);

      dequeued = queue.Dequeue();
      Assert.AreEqual(dequeued, string2);
      Assert.IsTrue(queue.Count == 0);

      queue.Enqueue(string1);
      queue.Enqueue(string2);
      Assert.IsTrue(queue.Contains(string1));

      queue.TrimToSize();
      var syncroot = queue.SyncRoot;
      Assert.IsNotNull(syncroot);

      Assert.Fail("Fix enumerator!");
      foreach (var item in queue)
      {
        Assert.IsTrue(item == string1 || item == string2);
      }
    }
  }
}
