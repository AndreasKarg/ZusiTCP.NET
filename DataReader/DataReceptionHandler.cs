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
    private readonly IEventInvocator<int> _eventManager;
    private readonly ITcpCommandDictionary _commandDictionary;

    private readonly IDictionary<int, IDataReader> _readersById;

    public IBinaryReader ClientReader { get; set; }

    public DataReceptionHandler(IDataReaderDictionary readersByType, IEventInvocator<int> eventManager,
      ITcpCommandDictionary commandDictionary, IDictionary<int, IDataReader> readersById)
    {
      _readersByType = readersByType;
      _eventManager = eventManager;
      _commandDictionary = commandDictionary;
      _readersById = readersById;
    }

    public int HandleData(int id)
    {
      IDataReader reader = GetReader(id);

      Debug.Assert(ClientReader != null);

      return reader.ReadDataAndInvokeEvents(id, ClientReader, _eventManager);
    }

    private void AcquireNewReader(int id)
    {
      var command = _commandDictionary[id];
      var reader = _readersByType[command.Type];

      _readersById[id] = reader;
    }

    private IDataReader GetReader(int id)
    {
      IDataReader result;

      if (_readersById.TryGetValue(id, out result))
        return result;

      AcquireNewReader(id);

      bool readerNowExists = _readersById.TryGetValue(id, out result);
      Debug.Assert(readerNowExists); // Todo: Make sure this can't be broken in release builds

      return result;
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
    IDataReceptionHandler Create(IEventInvocator<int> eventManager,
      ITcpCommandDictionary commandDictionary);
  }
}