#region header
// /*************************************************************************
//  * GenericTypedMethodList.cs
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

namespace Zusi_Datenausgabe
{
  public interface ITypedMethodList<T> : ITypedMethodListBase where T: EventArgs
  {
    void Subscribe(EventHandler<T> handler);
    void Unsubscribe(EventHandler<T> handler);
    void Invoke(object sender, T eventArgs);
  }

  public class TypedMethodList<T> : ITypedMethodList<T>, IDisposable where T : EventArgs
  {
    // TODO: Find out how to inject this kind of thing...
    private List<EventHandler<T>> _handlers = new List<EventHandler<T>>();

    #region Implementation of ITypedMethodList<>

    public void Subscribe(EventHandler<T> handler)
    {
      // TODO: Find out how regular events behave in this case
      if (ContainsHandler(handler))
        return;

      _handlers.Add(handler);
    }

    public void Unsubscribe(EventHandler<T> handler)
    {
      // TODO: Find out how regular events behave in this case
      if (!ContainsHandler(handler))
        return;

      _handlers.Remove(handler);
    }

    public void Invoke(object sender, T eventArgs)
    {
      foreach (var eventHandler in _handlers)
      {
        eventHandler(sender, eventArgs);
      }
    } 
    #endregion

    private bool ContainsHandler(EventHandler<T> handler)
    {
      return _handlers.Contains(handler);
    }

    #region Implementation of IDisposable

    public void Dispose()
    {
      _handlers.Clear();
    }

    #endregion
  }

  public interface ITypedMethodListFactory
  {
    void Release<T>(ITypedMethodList<T> list) where T: EventArgs;
    ITypedMethodList<T> Create<T>() where T:EventArgs;
  }

  public interface ITypedMethodListBase {}
}