#region header
// /*************************************************************************
//  * TypedEventManager.cs
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

namespace Zusi_Datenausgabe
{
  public interface ITypedEventManager
  {
    void Subscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler);
    void Unsubscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler);
    void Invoke<T>(object sender, DataReceivedEventArgs<T> eventArgs);
  }

  public class TypedEventManager : ITypedEventManager
  {
    private IDictionary<Type, ITypedMethodListBase> _eventHandlers;
    private ITypedMethodListFactory _methodListFactory;

    public TypedEventManager(IDictionary<Type, ITypedMethodListBase> eventHandlers, ITypedMethodListFactory methodListFactory)
    {
      _eventHandlers = eventHandlers;
      _methodListFactory = methodListFactory;
    }

    public void Subscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      var handlerList = GetDictionaryEntry<T>();
      handlerList.Subscribe(handler);
    }

    public void Unsubscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      ITypedMethodList<DataReceivedEventArgs<T>> handlerList;
      var entryExists = TryGetDictionaryEntry(typeof (T), out handlerList);

      if (!entryExists)
        return;

      handlerList.Unsubscribe(handler);
    }

    public void Invoke<T>(object sender, DataReceivedEventArgs<T> eventArgs)
    {
      ITypedMethodList<DataReceivedEventArgs<T>> handlerList;
      var entryExists = TryGetDictionaryEntry(typeof(T), out handlerList);

      if (!entryExists)
        return;

      handlerList.Invoke(sender, eventArgs);
    }

    private ITypedMethodList<DataReceivedEventArgs<T>> GetDictionaryEntry<T>()
    {
      var type = typeof (T);
      ITypedMethodListBase handlerList;

      ITypedMethodList<DataReceivedEventArgs<T>> typedMethodList;
      if (TryGetDictionaryEntry(type, out typedMethodList))
        return typedMethodList;

      var newList = CreateNewList<T>(type);
      return newList;
    }

    private bool TryGetDictionaryEntry<T>(Type type, out ITypedMethodList<DataReceivedEventArgs<T>> result)
    {
      ITypedMethodListBase handlerList;
      var entryFound = _eventHandlers.TryGetValue(type, out handlerList);

      if (entryFound)
      {
        result = CastToListType<T>(handlerList);
        return true;
      }

      result = null;
      return false;
    }

    private ITypedMethodList<DataReceivedEventArgs<T>> CastToListType<T>(ITypedMethodListBase handlerList)
    {
      Debug.Assert(handlerList is ITypedMethodList<DataReceivedEventArgs<T>>);

      return handlerList as ITypedMethodList<DataReceivedEventArgs<T>>;
    }

    private ITypedMethodList<DataReceivedEventArgs<T>> CreateNewList<T>(Type type)
    {
      Debug.Assert(type == typeof(T));

      var newList = _methodListFactory.Create<DataReceivedEventArgs<T>>();
      _eventHandlers[type] = newList;
      return newList;
    }
  }
}