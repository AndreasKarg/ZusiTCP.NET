using System;

namespace Zusi_Datenausgabe.EventManager
{
  internal interface IEventManager<in TKey> : IEventInvoker<TKey>
  {
    void Subscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler);
    void Unsubscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler);
  }

  internal interface IEventInvoker<in TKey>
  {
    void Invoke<T>(TKey key, object sender, DataReceivedEventArgs<T> eventArgs);
  }

  class EventManager<TKey> : EventManagerBase<TKey>, IEventManager<TKey>
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
  }
}