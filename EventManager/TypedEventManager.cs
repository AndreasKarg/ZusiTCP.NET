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
using Zusi_Datenausgabe.TypedMethodList;

namespace Zusi_Datenausgabe.EventManager
{
  public interface ITypedEventSubscriber
  {
    void Subscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler);
    void Unsubscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler);
  }

  public interface ITypedEventInvocator
  {
    void Invoke<T>(object sender, DataReceivedEventArgs<T> eventArgs);
  }

  public interface ITypedEventManager : ITypedEventSubscriber, ITypedEventInvocator
  {
  }

  public class TypedEventManager : EventManagerBase<Type>, ITypedEventManager
  {
    public TypedEventManager(ITypedMethodListFactory methodListFactory) : base(methodListFactory)
    {
    }

    #region Implementation of ITypedEventInvocator

    public void Invoke<T>(object sender, DataReceivedEventArgs<T> eventArgs)
    {
      Invoke(typeof(T), sender, eventArgs);
    }

    #endregion

    #region Implementation of ITypedEventManager

    public void Subscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      Subscribe(typeof(T), handler);
    }

    public void Unsubscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      Unsubscribe(typeof(T), handler);
    }

    #endregion

    protected override bool TryGetTypeForKey(Type key, out Type type)
    {
      type = key;
      return true;
    }
  }
}