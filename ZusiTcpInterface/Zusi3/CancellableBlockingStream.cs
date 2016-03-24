using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace ZusiTcpInterface.Zusi3
{
  public class CancellableBlockingStream : Stream
  {
    private readonly Stream _underlyingStream;
    private readonly CancellationToken _cancellationToken;

    public CancellableBlockingStream(Stream underlyingStream, CancellationToken cancellationToken)
    {
      _underlyingStream = underlyingStream;
      _cancellationToken = cancellationToken;
    }

    #region Delegating members

    public new object GetLifetimeService()
    {
      return _underlyingStream.GetLifetimeService();
    }

    public override object InitializeLifetimeService()
    {
      return _underlyingStream.InitializeLifetimeService();
    }

    public new ObjRef CreateObjRef(Type requestedType)
    {
      return _underlyingStream.CreateObjRef(requestedType);
    }

    public new Task CopyToAsync(Stream destination)
    {
      return _underlyingStream.CopyToAsync(destination);
    }

    public new Task CopyToAsync(Stream destination, int bufferSize)
    {
      return _underlyingStream.CopyToAsync(destination, bufferSize, _cancellationToken);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      return _underlyingStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public new void CopyTo(Stream destination)
    {
      WaitForTaskAndRethrowExceptions(_underlyingStream.CopyToAsync(destination));
    }

    public new void CopyTo(Stream destination, int bufferSize)
    {
      WaitForTaskAndRethrowExceptions(_underlyingStream.CopyToAsync(destination, bufferSize,_cancellationToken));
    }

    public override void Close()
    {
      _underlyingStream.Close();
    }

    public new void Dispose()
    {
      _underlyingStream.Dispose();
    }

    public override void Flush()
    {
      WaitForTaskAndRethrowExceptions(_underlyingStream.FlushAsync(_cancellationToken));
    }

    public new Task FlushAsync()
    {
      return _underlyingStream.FlushAsync(_cancellationToken);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      return _underlyingStream.FlushAsync(cancellationToken);
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      return _underlyingStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
      return _underlyingStream.EndRead(asyncResult);
    }

    public new Task<int> ReadAsync(byte[] buffer, int offset, int count)
    {
      return _underlyingStream.ReadAsync(buffer, offset, count, _cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return _underlyingStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      return _underlyingStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      _underlyingStream.EndWrite(asyncResult);
    }

    public new Task WriteAsync(byte[] buffer, int offset, int count)
    {
      return _underlyingStream.WriteAsync(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return _underlyingStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return _underlyingStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      _underlyingStream.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      var task = _underlyingStream.ReadAsync(buffer, offset, count, _cancellationToken);
      WaitForTaskAndRethrowExceptions(task);
      return task.Result;
    }

    public override int ReadByte()
    {
      var buf = new byte[1];
      var task = _underlyingStream.ReadAsync(buf, 0, 1, _cancellationToken);
      WaitForTaskAndRethrowExceptions(task);

      if (task.Result == -1)
        throw new EndOfStreamException();

      return buf[1];
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      WaitForTaskAndRethrowExceptions(_underlyingStream.WriteAsync(buffer, offset, count, _cancellationToken));
    }

    public override void WriteByte(byte value)
    {
      var buf = new[] {value};

      WaitForTaskAndRethrowExceptions(_underlyingStream.WriteAsync(buf, 0, buf.Length, _cancellationToken));
    }

    public override bool CanRead
    {
      get { return _underlyingStream.CanRead; }
    }

    public override bool CanSeek
    {
      get { return _underlyingStream.CanSeek; }
    }

    public override bool CanTimeout
    {
      get { return _underlyingStream.CanTimeout; }
    }

    public override bool CanWrite
    {
      get { return _underlyingStream.CanWrite; }
    }

    public override long Length
    {
      get { return _underlyingStream.Length; }
    }

    public override long Position
    {
      get { return _underlyingStream.Position; }
      set { _underlyingStream.Position = value; }
    }

    public override int ReadTimeout
    {
      get { return _underlyingStream.ReadTimeout; }
      set { _underlyingStream.ReadTimeout = value; }
    }

    public override int WriteTimeout
    {
      get { return _underlyingStream.WriteTimeout; }
      set { _underlyingStream.WriteTimeout = value; }
    }

    #endregion Delegating members

    private void WaitForTaskAndRethrowExceptions(Task task)
    {
      try
      {
        task.Wait(_cancellationToken);
      }
      catch (AggregateException aggregate)
      {
        foreach (var innerException in aggregate.InnerExceptions)
        {
          throw innerException;
        }
      }
    }
  }
}