#region header
// /*************************************************************************
//  * TypedAndGenericEventManager.cs
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

namespace Zusi_Datenausgabe.EventManager
{
  public interface ITypedAndGenericEventManager<TKey> : ITypedEventSubscriber, IEventManager<TKey>
  {
  }

  public class TypedAndGenericEventManager<TKey> : ITypedAndGenericEventManager<TKey>
  {
    private ITypedEventManager _typedManager;
    private IEventManager<TKey> _genericManager;

    public TypedAndGenericEventManager(ITypedEventManager typedManager, IEventManager<TKey> genericManager)
    {
      _typedManager = typedManager;
      _genericManager = genericManager;
    }

    public void Subscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _genericManager.Subscribe(key, handler);
    }

    public void Subscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _typedManager.Subscribe(handler);
    }

    public void Unsubscribe<T>(TKey key, EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _genericManager.Unsubscribe(key, handler);
    }

    public void Unsubscribe<T>(EventHandler<DataReceivedEventArgs<T>> handler)
    {
      _typedManager.Unsubscribe(handler);
    }

    public void Invoke<T>(TKey key, object sender, DataReceivedEventArgs<T> eventArgs)
    {
      _genericManager.Invoke(key, sender, eventArgs);
      _typedManager.Invoke(sender, eventArgs);
    }

    public void SetupTypeForKey<T>(TKey key)
    {
      _genericManager.SetupTypeForKey<T>(key);
    }

    public GetEventTypeDelegate<TKey> GetEventTypeDelegate { get; set; }
  }
}