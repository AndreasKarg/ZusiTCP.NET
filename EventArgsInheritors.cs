using System;

namespace Zusi_Datenausgabe
{
  /// <summary>
  /// Represents a structure containing the key and value of one dataset received via the TCP interface.
  /// </summary>
  /// <typeparam name="T">Type of this data set. May be <see cref="float"/>, <see cref="string"/> or <see cref="byte"/>[]</typeparam>
  public class DataReceivedEventArgs<T> : EventArgs
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DataReceivedEventArgs{T}"/> structure and fills the Id and Value fields with the values
    /// passed to the id and value parameters respectively.
    /// </summary>
    /// <param name="id">The id number of the measurement.</param>
    /// <param name="value">The value of the measurement.</param>
    public DataReceivedEventArgs(int id, T value)
    {
      Id = id;
      Value = value;
    }

    /// <summary>
    /// Gets the Zusi ID number of this dataset.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the new value of this dataset.
    /// </summary>
    public T Value { get; private set; }
  }

  public class ErrorEventArgs : EventArgs
  {
    private readonly ZusiTcpException _exception;

    public ErrorEventArgs(ZusiTcpException exception)
    {
      _exception = exception;
    }

    public ZusiTcpException Exception
    {
      get { return _exception; }
    }
  }
}