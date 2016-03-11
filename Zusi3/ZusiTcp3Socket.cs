namespace Zusi_Datenausgabe.Zusi3
{
  /// <summary>
  ///   Represents the delegate type required for event handling. Used to transfer incoming data sets to the client application.
  /// </summary>
  /// <param name="data">Contains the new dataset.</param>
  /// <param name="sender">Contains the object triggering the event.</param>
  public delegate void Node3ReceiveEvent(object sender, Node data);

  /// <summary>
  ///   Represents the delegate type required for error event handling. Used to handle exceptions that occur in the reception thread.
  /// </summary>
  /// <param name="ex">Contains the exception that has occured.</param>
  /// <param name="sender">Contains the object triggering the event.</param>
  public delegate void Error3Event(object sender, System.Exception ex);

  /// <summary>
  ///   Manages IO of a Streams based on Zusi3Protocol.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class ZusiTcp3Socket
  {
    //public ZusiTcp3Socket()
    //{
    //}

    /// <summary>
    ///   Writes the Node to the stream.
    /// </summary>
    public void SendKnoten(Node d)
    {
      d.Write(Stream);
    }

    private System.Net.Sockets.NetworkStream Internal_Stream = null;
    /// <summary>
    ///   Sets the stream to read from.
    /// </summary>
    public System.Net.Sockets.NetworkStream Stream
    {
      get
      {
        return Internal_Stream;
      }
      set
      {
        if (CurrentStreamWorking != null)
        {
          try
          {
            Stream.EndRead(CurrentStreamWorking);
          }
          catch
          {
          }
          Internal_Stream = value;
        }
        CurrentStreamWorking = null;
        Buffer = new byte[256];
        Internal_Stream = value;
        if (value != null)
          CurrentStreamWorking = Stream.BeginRead(Buffer, OldStartPosition, Buffer.Length - OldStartPosition, StreamAsyncCallback, null);
      }
    }

    private byte[] Buffer = new byte[256];
    private System.IAsyncResult CurrentStreamWorking = null;
    private Node CurrentNode = null;
    private int OldStartPosition = 0;
    private void On_ProcessKnoten(Node k)
    {
      ProcessKnoten.Invoke(this, k);
    }
    public event Node3ReceiveEvent ProcessKnoten;
    private void On_ReadException(System.Exception ex)
    {
      ReadException.Invoke(this, ex);
    }
    public event Error3Event ReadException;

    private void StreamAsyncCallback(System.IAsyncResult ar)
    {
      try
      {
        CurrentStreamWorking = null;
        int length = Stream.EndRead(ar) + OldStartPosition;
        int currentPosition = 0;
        bool needsMore = (length == Buffer.Length);
        Node oldcurrentnode = CurrentNode;
        if (length < 0)
            throw new System.Exception();

        while (true)
        {
          if (CurrentNode == null)
            CurrentNode = new Node();
          if (!CurrentNode.ReadTopNode(Buffer, ref currentPosition, ref length))
            break;
          if (length < 0)
            throw new System.Exception();
          On_ProcessKnoten(CurrentNode);
          CurrentNode = null;
        }
        if (length < 0)
          throw new System.Exception();

        needsMore = needsMore && (CurrentNode != null) && (CurrentNode == oldcurrentnode);

        if ((length > 0) || needsMore)
        {
            //Not everything successfull read
            if ((currentPosition < length) || needsMore)
                System.Array.Copy(Buffer, currentPosition, Buffer, 0, length);
            else
            {
              byte[] barr2;
              if (needsMore)
                barr2 = new byte[Buffer.Length + 256];
              else
                barr2 = new byte[Buffer.Length];

              System.Array.Copy(Buffer, currentPosition, barr2, 0, length);
              Buffer = barr2;
            }
        }
        OldStartPosition = length;
        CurrentStreamWorking = Stream.BeginRead(Buffer, OldStartPosition, Buffer.Length - OldStartPosition, StreamAsyncCallback, null);
      }
      catch (System.Exception ex)
      {
        On_ReadException(ex);
      }
    }
  }
}
