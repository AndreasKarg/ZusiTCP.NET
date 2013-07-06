using System;
using Zusi_Datenausgabe.TypedMethodList;

namespace Zusi_Datenausgabe.EventManager
{
  public interface IEventSubscriber<in TKey>
  {
    void Subscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler);
    void Unsubscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler);
  }

  public interface IEventInvocator<in TKey>
  {
    void Invoke<T>(TKey key, object sender, DataReceivedEventArgs<T> eventArgs);
  }

  public interface IEventManager<in TKey> : IEventSubscriber<TKey>, IEventInvocator<TKey>
  {
    void SetupTypeForKey<T>(TKey key);
  }

  public class EventManager<TKey> : EventManagerBase<TKey>, IEventManager<TKey>
  {
    public EventManager(ITypedMethodListFactory methodListFactory) : base(methodListFactory)
    {
    }

    public new void Subscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      base.Subscribe(key, handler);
    }

    public new void Unsubscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      base.Unsubscribe(key, handler);
    }

    public new void Invoke<T>(TKey key, object sender, DataReceivedEventArgs<T> eventArgs)
    {
      base.Invoke(key, sender, eventArgs);
    }

    public void SetupTypeForKey<T>(TKey key)
    {
      try
      {
        GetDictionaryEntry<T>(key);
      }
      catch (InvalidCastException ex)
      {
        throw new ArgumentException(string.Format("A different type has already been registered for key {0}. " +
                                                  "See inner exception for details.", key), "key", ex);
      }
    }
  }


}