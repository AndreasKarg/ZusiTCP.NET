using System.Threading;

namespace Zusi_Datenausgabe.EventManager
{
  // TODO: Maybe make the marshal generic and have a reference next to each event?
  public class EventMarshal<TKey> : IEventInvocator<TKey>
  {
    private readonly SynchronizationContext _hostContext;
    private readonly IEventInvocator<TKey> _invocator;

    public EventMarshal(SynchronizationContext hostContext, IEventInvocator<TKey> invocator)
    {
      _hostContext = hostContext;
      _invocator = invocator;
    }

    public void Invoke<T>(TKey key, object sender, DataReceivedEventArgs<T> eventArgs)
    {
      PostToHost(key, sender, eventArgs);
    }

    private void PostToHost<T>(TKey key, object sender, DataReceivedEventArgs<T> value)
    {
      _hostContext.Post(MarshalEvent<T>, new MarshalArgs<T>(value, sender, key));
    }

    private void MarshalEvent<T>(object o)
    {
      var margs = (MarshalArgs<T>)o;
      _invocator.Invoke(margs.Key, margs.Sender, margs.Data);
    }

    private struct MarshalArgs<TData>
    {
      public DataReceivedEventArgs<TData> Data { get; private set; }

      public object Sender { get; private set; }

      public TKey Key { get; private set; }

      public MarshalArgs(DataReceivedEventArgs<TData> data, object sender, TKey key)
        : this()
      {
        Data = data;
        Sender = sender;
        Key = key;
      }
    }
  }

  public interface IEventMarshalFactory
  {
    EventMarshal<TKey> Create<TKey>(SynchronizationContext hostContext, IEventInvocator<TKey> invocator);
  }
}