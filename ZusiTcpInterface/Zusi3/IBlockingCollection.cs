using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ZusiTcpInterface.Zusi3
{
  internal interface IBlockingCollection<T> : ICollection, IDisposable, IReadOnlyCollection<T>
  {
    int BoundedCapacity { get; }
    bool IsAddingCompleted { get; }
    bool IsCompleted { get; }
    void Add(T item);
    void Add(T item, CancellationToken cancellationToken);
    bool TryAdd(T item);
    bool TryAdd(T item, TimeSpan timeout);
    bool TryAdd(T item, int millisecondsTimeout);
    bool TryAdd(T item, int millisecondsTimeout, CancellationToken cancellationToken);
    T Take();
    T Take(CancellationToken cancellationToken);
    bool TryTake(out T item);
    bool TryTake(out T item, TimeSpan timeout);
    bool TryTake(out T item, int millisecondsTimeout);
    bool TryTake(out T item, int millisecondsTimeout, CancellationToken cancellationToken);
    void CompleteAdding();
    T[] ToArray();
    void CopyTo(T[] array, int index);
    IEnumerable<T> GetConsumingEnumerable();
    IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken);
  }
}