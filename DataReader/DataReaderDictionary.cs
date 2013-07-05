#region header
// /*************************************************************************
//  * DataReaderDictionary.cs
//  * Contains logic for the TCP interface.
//  * 
//  * (C) 2013-2013 Andreas Karg, <Clonkman@gmx.de>
//  * 
//  * This file is part of Zusi TCP Interface.NET.
//  *
//  * Zusi TCP Interface.NET is free software: you can redistribute it and/or
//  * modify it under the terms of the GNU General Public License as
//  * published by the Free Software Foundation, either version 3 of the
//  * License, or (at your option) any later version.
//  *
//  * Zusi TCP Interface.NET is distributed in the hope that it will be
//  * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  * GNU General Public License for more details.
//  *
//  * You should have received a copy of the GNU General Public License
//  * along with Zusi TCP Interface.NET. 
//  * If not, see <http://www.gnu.org/licenses/>.
//  * 
//  *************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Zusi_Datenausgabe.DataReader
{
  public interface IDataReaderDictionary
  {
    IDataReader GetReader(string inputType);

    IDataReader this[string inputType]
    {
      [DebuggerStepThrough]
      get;
    }
  }

  public class DataReaderDictionary : IDataReaderDictionary
  {
    private readonly IDictionary<string, IDataReader> _readers;

    public DataReaderDictionary(IEnumerable<IDataReader> readers, IDictionary<string, IDataReader> readerDictionary)
    {
      _readers = readerDictionary;

      InitializeReaders(readers);
    }

    private void InitializeReaders(IEnumerable<IDataReader> readers)
    {
      foreach (var reader in readers)
      {
        var inputType = reader.InputType;
        if(_readers.ContainsKey(inputType))
          throw new DataReaderException("A reader for input type " + inputType +
                                        " has already been registered by " + _readers[inputType].GetType());

        _readers[inputType] = reader;
      }
    }

    public IDataReader GetReader(string inputType)
    {
      try
      {
        return _readers[inputType];
      }
      catch (KeyNotFoundException ex)
      {
        throw new ArgumentException("No reader was registered for input type " + inputType, "inputType", ex);
      }
    }

    public IDataReader this[string inputType]
    {
      [DebuggerStepThrough]
      get { return GetReader(inputType); }
    }
  }

  [Serializable]
  public class DataReaderException : Exception
  {
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public DataReaderException()
    {
    }

    public DataReaderException(string message) : base(message)
    {
    }

    public DataReaderException(string message, Exception inner) : base(message, inner)
    {
    }

    protected DataReaderException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
  }
}