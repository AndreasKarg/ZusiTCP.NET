#region header
// /*************************************************************************
//  * DataHandlerDictionary.cs
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

namespace Zusi_Datenausgabe.DataHandler
{
  public interface IDataHandlerDictionary
  {
  }

  public class DataHandlerDictionary : IDataHandlerDictionary
  {
    private readonly IDictionary<string, IDataHandler> _handlers;

    public DataHandlerDictionary(IEnumerable<IDataHandler> handlerClasses, IDictionary<string, IDataHandler> handlers)
    {
      _handlers = handlers;

      InitializeHandlers(handlerClasses);
    }

    private void InitializeHandlers(IEnumerable<IDataHandler> handlerList)
    {
      foreach (var dataHandler in handlerList)
      {
        var inputType = dataHandler.InputType;
        if(_handlers.ContainsKey(inputType))
          throw new DataHandlerException("A handler for input type " + inputType +
                                        " has already been registered by " + _handlers[inputType].GetType());

        _handlers[inputType] = dataHandler;
      }
    }

    public IDataHandler GetHandler(string inputType)
    {
      try
      {
        return _handlers[inputType];
      }
      catch (KeyNotFoundException ex)
      {
        throw new ArgumentException("No handler was registered for input type " + inputType, "inputType", ex);
      }
    }

    public IDataHandler this[string inputType]
    {
      [DebuggerStepThrough]
      get { return GetHandler(inputType); }
    }
  }

  [Serializable]
  public class DataHandlerException : Exception
  {
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public DataHandlerException()
    {
    }

    public DataHandlerException(string message) : base(message)
    {
    }

    public DataHandlerException(string message, Exception inner) : base(message, inner)
    {
    }

    protected DataHandlerException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
  }
}