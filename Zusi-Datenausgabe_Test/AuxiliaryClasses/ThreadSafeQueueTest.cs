using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zusi_Datenausgabe_Test.AuxiliaryClasses
{
  [TestClass]
  public class ThreadSafeQueueTest
  {
    [TestMethod]
    public void TestEnqueueDequeueWithString()
    {
      var queue = new ThreadSafeQueue<string>();

      string string1 = "String 1";
      string string2 = "String 2";

      queue.Enqueue(string1);
      Assert.IsTrue(queue.Count == 1, "Queue count is not 1.");

      queue.Enqueue(string2);
      Assert.IsTrue(queue.Count == 2, "Queue count is not 2.");

      var dequeued = queue.Dequeue();
      Assert.AreEqual(dequeued, string1);
      Assert.IsTrue(queue.Count == 1);

      dequeued = queue.Peek() as String;
      Assert.Fail("Typecast Peek!");
      Assert.AreEqual(dequeued, string2);
      Assert.IsTrue(queue.Count == 1);

      dequeued = queue.Dequeue();
      Assert.AreEqual(dequeued, string2);
      Assert.IsTrue(queue.Count == 0);
    }
  }
}
