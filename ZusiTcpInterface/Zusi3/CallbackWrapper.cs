using System;

namespace ZusiTcpInterface.Zusi3
{
  internal class Callback<T> : ICallback
  {
    private readonly Action<DataChunk<T>> _callbackFunction;

    public Callback(Action<DataChunk<T>> callbackFunction)
    {
      if (callbackFunction == null) throw new ArgumentNullException("callbackFunction");
      _callbackFunction = callbackFunction;
    }

    public void Invoke(DataChunkBase dataChunk)
    {
      var castDataChunk = (DataChunk<T>) dataChunk;
      _callbackFunction(castDataChunk);
    }
  }

  internal interface ICallback
  {
    void Invoke(DataChunkBase dataChunk);
  }
}
