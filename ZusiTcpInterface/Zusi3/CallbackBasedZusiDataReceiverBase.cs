using System;
using System.Collections.Generic;

namespace ZusiTcpInterface.Zusi3
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public abstract class CallbackBasedZusiDataReceiverBase
  {
    private readonly Dictionary<Address, ICallback> _callbacks = new Dictionary<Address, ICallback>();

    protected void RaiseEventFor(DataChunkBase chunk)
    {
      var address = chunk.Address;
      if (!_callbacks.ContainsKey(address))
        return;

      var callback = _callbacks[address];
      callback.Invoke(chunk);
    }

    public void RegisterCallbackFor<T>(Address address, Action<DataChunk<T>> callback)
    {
      try
      {
        _callbacks.Add(address, new Callback<T>(callback));
      }
      catch (ArgumentException e)
      {
        throw new ArgumentException(String.Format("A callback for address {0} has already been defined.", address), "address", e);
      }
    }
  }
}
