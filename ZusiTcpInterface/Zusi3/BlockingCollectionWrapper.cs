using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ZusiTcpInterface.Zusi3
{
  public class BlockingCollectionWrapper<T> : IBlockingCollection<T>
  {
    private readonly BlockingCollection<T> _blockingCollection = new BlockingCollection<T>();

    public IEnumerator<T> GetEnumerator()
    {
      return ((IEnumerable<T>) _blockingCollection).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) _blockingCollection).GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
      ((ICollection) _blockingCollection).CopyTo(array, index);
    }

    public object SyncRoot
    {
      get { return ((ICollection) _blockingCollection).SyncRoot; }
    }

    public bool IsSynchronized
    {
      get { return ((ICollection) _blockingCollection).IsSynchronized; }
    }

    public void Add(T item)
    {
      _blockingCollection.Add(item);
    }

    public void Add(T item, CancellationToken cancellationToken)
    {
      _blockingCollection.Add(item, cancellationToken);
    }

    public bool TryAdd(T item)
    {
      return _blockingCollection.TryAdd(item);
    }

    public bool TryAdd(T item, TimeSpan timeout)
    {
      return _blockingCollection.TryAdd(item, timeout);
    }

    public bool TryAdd(T item, int millisecondsTimeout)
    {
      return _blockingCollection.TryAdd(item, millisecondsTimeout);
    }

    public bool TryAdd(T item, int millisecondsTimeout, CancellationToken cancellationToken)
    {
      return _blockingCollection.TryAdd(item, millisecondsTimeout, cancellationToken);
    }

    public T Take()
    {
      return _blockingCollection.Take();
    }

    public T Take(CancellationToken cancellationToken)
    {
      return _blockingCollection.Take(cancellationToken);
    }

    public bool TryTake(out T item)
    {
      return _blockingCollection.TryTake(out item);
    }

    public bool TryTake(out T item, TimeSpan timeout)
    {
      return _blockingCollection.TryTake(out item, timeout);
    }

    public bool TryTake(out T item, int millisecondsTimeout)
    {
      return _blockingCollection.TryTake(out item, millisecondsTimeout);
    }

    public bool TryTake(out T item, int millisecondsTimeout, CancellationToken cancellationToken)
    {
      return _blockingCollection.TryTake(out item, millisecondsTimeout, cancellationToken);
    }

    public void CompleteAdding()
    {
      _blockingCollection.CompleteAdding();
    }

    public void Dispose()
    {
      _blockingCollection.Dispose();
    }

    public T[] ToArray()
    {
      return _blockingCollection.ToArray();
    }

    public void CopyTo(T[] array, int index)
    {
      _blockingCollection.CopyTo(array, index);
    }

    public IEnumerable<T> GetConsumingEnumerable()
    {
      return _blockingCollection.GetConsumingEnumerable();
    }

    public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
    {
      return _blockingCollection.GetConsumingEnumerable(cancellationToken);
    }

    public int BoundedCapacity
    {
      get { return _blockingCollection.BoundedCapacity; }
    }

    public bool IsAddingCompleted
    {
      get { return _blockingCollection.IsAddingCompleted; }
    }

    public bool IsCompleted
    {
      get { return _blockingCollection.IsCompleted; }
    }

    public int Count
    {
      get { return _blockingCollection.Count; }
    }
  }
}