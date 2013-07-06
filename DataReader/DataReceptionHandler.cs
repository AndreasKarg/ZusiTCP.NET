using System.Collections.Generic;
using System.Diagnostics;
using Zusi_Datenausgabe.EventManager;
using Zusi_Datenausgabe.NetworkIO;
using Zusi_Datenausgabe.TcpCommands;

namespace Zusi_Datenausgabe.DataReader
{
  public interface IDataReceptionHandler
  {
    int HandleData(int id);

    IBinaryReader ClientReader
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    }
  }

  public class DataReceptionHandler : IDataReceptionHandler
  {
    private readonly IDataReaderDictionary _readersByType;
    private readonly IEventInvoker<int> _eventManager;
    private readonly ITcpCommandDictionary _commandDictionary;

    private readonly IDictionary<int, IDataReader> _readersById;

    public IBinaryReader ClientReader { get; set; }

    public DataReceptionHandler(IDataReaderDictionary readersByType, IEventInvoker<int> eventManager,
      ITcpCommandDictionary commandDictionary, IDictionary<int, IDataReader> readersById)
    {
      _readersByType = readersByType;
      _eventManager = eventManager;
      _commandDictionary = commandDictionary;
      _readersById = readersById;
    }

    public int HandleData(int id)
    {
      int bytesRead;
      bool success = TryUseSavedReader(id, out bytesRead);

      if (success)
        return bytesRead;

      AcquireNewReader(id);

      success = TryUseSavedReader(id, out bytesRead);

      Debug.Assert(success);

      return bytesRead;
    }

    private bool TryUseSavedReader(int id, out int bytesRead)
    {
      IDataReader reader;

      if (!_readersById.TryGetValue(id, out reader))
      {
        bytesRead = 0;
        return false;
      }

      Debug.Assert(ClientReader != null);
      bytesRead = reader.ReadDataAndInvokeEvents(id, ClientReader, _eventManager);

      return true;
    }

    private void AcquireNewReader(int id)
    {
      var command = _commandDictionary[id];
      var reader = _readersByType[command.Type];

      _readersById[id] = reader;
    }

    private struct MarshalArgs<T>
    {
      public DataReceivedEventArgs<T> Data { get; private set; }

      public MarshalArgs(int id, T data)
        : this()
      {
        Data = new DataReceivedEventArgs<T>(id, data);
      }
    }

    private void EventMarshal<T>(object o)
    {
      var margs = (MarshalArgs<T>)o;
      _eventManager.Invoke(margs.Data.Id, this, margs.Data);
    }
  }

  public interface IDataReceptionHandlerFactory
  {
    IDataReceptionHandler Create(IEventInvoker<int> eventManager,
      ITcpCommandDictionary commandDictionary);
  }
}