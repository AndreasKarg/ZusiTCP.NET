namespace Zusi_Datenausgabe
{
  /// <summary>
  ///   Represents a knode for all TCPconnections based on the Zusi-protocol.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class ZusiTcp3Knode
  {
    public ZusiTcp3Knode()
    {
      Attributes = new System.Collections.Generic.List<ZusiTcp3AttributeAbstract>();
      Knodes = new System.Collections.Generic.List<ZusiTcp3Knode>();
      IsReading = false;
    }
    /// <summary>Specifys the purpose of all subelements</summary>
    public System.Int16 ID {set; get;}
    /// <summary>Contains all child attributes</summary>
    public System.Collections.Generic.List<ZusiTcp3AttributeAbstract> Attributes {private set; get;}
    /// <summary>Contains all child knodes</summary>
    public System.Collections.Generic.List<ZusiTcp3Knode> Knodes {private set; get;}
    /// <summary>true if the last <see cref="Read" /> did not finish.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public bool IsReading {private set; get;}
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    private bool ReadingIDIsSet {set; get;}
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    private System.Int32 CurrentAttributeLength {set; get;}

    public ZusiTcp3Knode TryGetSubKnode(System.Int16 id)
    {
      foreach(ZusiTcp3Knode k1 in Knodes)
      {
        if (k1.ID == id)
          return k1;
      }
      return null;
    }
    public ZusiTcp3AttributeAbstract TryGetSubAttribute(System.Int16 id)
    {
      foreach(ZusiTcp3AttributeAbstract a1 in Attributes)
      {
        if (a1.ID == id)
          return a1;
      }
      return null;
    }

    public ZusiTcp3Knode AddSubKnode(System.Int16 id)
    {
      var value = new ZusiTcp3Knode();
      value.ID = id;
      Knodes.Add(value);
      return value;
    }
    public ZusiTcp3AttributeAbstract AddSubAttribute(System.Int16 id)
    {
      var value = new ZusiTcp3AttributeAbstract();
      value.ID = id;
      Attributes.Add(value);
      return value;
    }

    public bool ReadTopKnode(byte[] Buffer, ref int Offset, ref int BufferLength)
    {
      if (!IsReading)
      {
        if (BufferLength < 4)
          return false;
        System.Int64 currentLength2 = Buffer[Offset] + 0x100 * Buffer[Offset + 1] + 0x10000 * Buffer[Offset + 2] + ((System.Int64) 0x1000000) * Buffer[Offset + 3];
        if (currentLength2 > System.Int32.MaxValue)
          currentLength2 -= 0x100000000;
        System.Int32 currentLength = (System.Int32) currentLength2;
        if (currentLength != 0)
          throw new System.IO.InvalidDataException();
        Offset += 4;
        BufferLength -= 4;
      }
      return Read(Buffer, ref Offset, ref BufferLength);
    }

    public bool Read(byte[] Buffer, ref int Offset, ref int BufferLength)
    {
      if (!IsReading)
      {
        ReadingIDIsSet = false;
        IsReading = true;
        Attributes.Clear();
        Knodes.Clear();
      }
      if (BufferLength < 2)
        return false;
      if (!ReadingIDIsSet)
      {
        ID = (System.Int16) (Buffer[Offset] + 0x100 * Buffer[Offset + 1]);
        ReadingIDIsSet = true;
        Offset += 2;
        BufferLength -= 2;
      }
      foreach (ZusiTcp3Knode k1 in Knodes)
      {
        if (k1.IsReading)
        {
          if (!k1.Read(Buffer, ref Offset, ref BufferLength))
            return false;
        }
      }
      foreach (ZusiTcp3AttributeAbstract a1 in Attributes)
      {
        if (a1.IsReading)
        {
          if (!a1.Read(CurrentAttributeLength, Buffer, ref Offset, ref BufferLength))
            return false;
        }
      }
      while (true)
      {
        if (BufferLength < 4)
          return false;
        System.Int64 currentLength2 = Buffer[Offset] + 0x100 * Buffer[Offset + 1] + 0x10000 * Buffer[Offset + 2] + ((System.Int64) 0x1000000) * Buffer[Offset + 3];
        if (currentLength2 > System.Int32.MaxValue)
          currentLength2 -= 0x100000000;
        System.Int32 currentLength = (System.Int32) currentLength2;
        Offset += 4;
        BufferLength -= 4;
        if (currentLength == 0)
        {
          var knoten1 = new ZusiTcp3Knode();
          Knodes.Add(knoten1);
          if (!knoten1.Read(Buffer, ref Offset, ref BufferLength))
             return false;
        }
        else if (currentLength == -1)
        {
          IsReading = false;
          return true;
        }
        else if (currentLength < 0)
          throw new System.IO.InvalidDataException();
        else
        {
          CurrentAttributeLength = currentLength;
          var attribute1 = new ZusiTcp3AttributeAbstract();
          Attributes.Add(attribute1);
          if (!attribute1.Read(CurrentAttributeLength, Buffer, ref Offset, ref BufferLength))
            return false;
        }
      }
      throw new System.Exception();
    }
    public void Write(System.IO.Stream Output)
    {
        byte[] outp = System.BitConverter.GetBytes((System.Int32) 0);
        Output.Write(outp, 0, 4);
        outp = System.BitConverter.GetBytes((System.Int16) ID);
        if (!System.BitConverter.IsLittleEndian)
        {
            byte b0 = outp[1];
            outp[1] = outp[0];
            outp[0] = b0;
        }
        Output.Write(outp, 0, 2);
        foreach (ZusiTcp3Knode k1 in Knodes)
        {
            k1.Write(Output);
        }
        foreach (ZusiTcp3AttributeAbstract a1 in Attributes)
        {
            a1.Write(Output);
        }
        outp = System.BitConverter.GetBytes((System.Int32) (-1));
        Output.Write(outp, 0, 4);
    }
  }

  /// <summary>
  ///   Represents a attribute for all TCPconnections based on the Zusi-protocol.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public class ZusiTcp3AttributeAbstract
  {
    public ZusiTcp3AttributeAbstract()
    {
      Data = new byte[] {};
      IsReading = false;
    }
    /// <summary>Specifys the purpose of this data</summary>
    public System.Int16 ID {set; get;}
    /// <summary>Returns the data in raw format</summary>
    public byte[] Data {set; get;}
    /// <summary>true if the last <see cref="Read" /> did not finish.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public bool IsReading {private set; get;}


    public float DataAsSingle
    {
      get
      {
        if (Data.Length != 4)
          throw new System.InvalidOperationException();
        return System.BitConverter.ToSingle(Data, 0);
      }
      set
      {
        Data = System.BitConverter.GetBytes(value);
      }
    }
    public System.Int16 DataAsInt16
    {
      get
      {
        if (Data.Length != 2)
          throw new System.InvalidOperationException();
        return (System.Int16) (Data[0] + 0x100 * Data[1]);
      }
      set
      {
        Data = System.BitConverter.GetBytes((System.Int16)value);
        if (!System.BitConverter.IsLittleEndian)
        {
            byte b0 = Data[1];
            Data[1] = Data[0];
            Data[0] = b0;
        }
      }
    }
    public byte DataAsByte
    {
      get
      {
        if (Data.Length != 1)
          throw new System.InvalidOperationException();
        return Data[0];
      }
      set
      {
        Data = new byte[] {value};
      }
    }
    public string DataAsString
    {
      get
      {
        return System.Text.Encoding.UTF8.GetString(Data);
      }
      set
      {
        Data = System.Text.Encoding.UTF8.GetBytes(value);
      }
    }

    public bool Read(System.Int32 PaketLength, byte[] Buffer, ref int Offset, ref int BufferLength)
    {
      if (PaketLength < 2)
        throw new System.IO.InvalidDataException();

      if (BufferLength < PaketLength)
      {
        IsReading = true;
        return false;
      }
      System.Int32 val0 = (System.Int32) (Buffer[Offset] + 0x100 * Buffer[Offset + 1]);
      if (val0 > System.Int16.MaxValue)
        val0 -= 0x10000;
      ID = (System.Int16) val0;
      Data = new byte[PaketLength - 2];
      System.Array.Copy(Buffer, Offset + 2, Data, 0, PaketLength - 2);
      IsReading = false;
      Offset += PaketLength;
      BufferLength -= PaketLength;
      return true;
    }
    public void Write(System.IO.Stream Output)
    {
      byte[] outp;
      outp = System.BitConverter.GetBytes((System.Int32)(Data.Length + 2));
      if (!System.BitConverter.IsLittleEndian)
      {
        byte b0 = outp[3];
        outp[3] = outp[0];
        outp[0] = b0;
        b0 = outp[2];
        outp[2] = outp[1];
        outp[1] = b0;
      }
      Output.Write(outp, 0, 4);
      outp = System.BitConverter.GetBytes((System.Int16)ID);
      if (!System.BitConverter.IsLittleEndian)
      {
        byte b0 = outp[1];
        outp[1] = outp[0];
        outp[0] = b0;
      }
      Output.Write(outp, 0, 2);
      Output.Write(Data, 0, Data.Length);
    }
  }
}
