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
    private readonly IEventInvocator<int> _eventManager;

    private readonly IDataReaderManager _dataReaderManager;

    public IBinaryReader ClientReader { get; set; }

    public DataReceptionHandler(IEventInvocator<int> eventManager, IDataReaderManager dataReaderManager)
    {
      _eventManager = eventManager;
      _dataReaderManager = dataReaderManager;
    }

    public int HandleData(int id)
    {
      IDataReader reader = _dataReaderManager.GetReader(id);

      Debug.Assert(ClientReader != null);

      return reader.ReadDataAndInvokeEvents(id, ClientReader, _eventManager);
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
      var margs = (DataReceptionHandler.MarshalArgs<T>)o;
      _eventManager.Invoke(margs.Data.Id, this, margs.Data);
    }
  }

  public interface IDataReceptionHandlerFactory
  {
    IDataReceptionHandler Create(IEventInvocator<int> eventManager,
      ITcpCommandDictionary commandDictionary);
  }
}