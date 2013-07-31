using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Zusi_Datenausgabe_Test.AuxiliaryClasses
{
  internal class ThreadSafeQueue<T>
  {
    private readonly Queue _queue = Queue.Synchronized(new Queue());

    public void Clear()
    {
      _queue.Clear();
    }

    public void CopyTo(Array array, int index)
    {
      _queue.CopyTo(array, index);
    }

    public void Enqueue(T obj)
    {
      _queue.Enqueue(obj);
    }

    public IEnumerator GetEnumerator()
    {
      return _queue.GetEnumerator() as IEnumerator<T>;
    }

    public T Dequeue()
    {
      return (T) _queue.Dequeue();
    }

    public object Peek()
    {
      return _queue.Peek();
    }

    public bool Contains(T obj)
    {
      return _queue.Contains(obj);
    }

    public T[] ToArray()
    {
      return _queue.ToArray() as T[];
    }

    public void TrimToSize()
    {
      _queue.TrimToSize();
    }

    public int Count
    {
      get { return _queue.Count; }
    }

    public bool IsSynchronized
    {
      get { return _queue.IsSynchronized; }
    }

    public object SyncRoot
    {
      get { return _queue.SyncRoot; }
    }
  }

  public class SimpleSynchronizationContext : SynchronizationContext
  {
    private ThreadSafeQueue<Tuple<SendOrPostCallback, object>> eventQueue = new ThreadSafeQueue<Tuple<SendOrPostCallback, object>>();

    public override void Post(SendOrPostCallback d, object state)
    {
      eventQueue.Enqueue(new Tuple<SendOrPostCallback, object>(d, state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
      throw new NotSupportedException();
    }

    public bool HandleOne()
    {
      if (eventQueue.Count == 0)
        return false;

      var item = eventQueue.Dequeue();

      item.Item1(item.Item2);

      return true;
    }

    public void HandleAll()
    {
      while (HandleOne()) ;
    }
  }
}
