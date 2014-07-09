#region header

/*************************************************************************
 * BlockingList.cs
 * Contains a List type whose contents can be set to readonly by the owner.
 * 
 * (C) 2013-2013 Andreas Karg, <Clonkman@gmx.de>
 * 
 * This file is part of Zusi TCP Interface.NET.
 *
 * Zusi TCP Interface.NET is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * Zusi TCP Interface.NET is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Zusi TCP Interface.NET. 
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 *************************************************************************/

#endregion

#region Using

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Zusi_Datenausgabe
{
  public static class CollectionHelper
  {
    private static void AddRangeManual<T>(ICollection<T> target, IEnumerable<T> source)
    {
      foreach (var item in source)
      {
        target.Add(item);
      }
    }

    public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> collection)
    {
      var selfList = self as List<T>;

      if (selfList != null)
      {
        selfList.AddRange(collection);
      }
      else
      {
        AddRangeManual(self, collection);
      }
    }
  }

  public class SwitchableReadOnlyList<T> : ICollection<T>
  {
    private readonly List<T> _list;

    public SwitchableReadOnlyList(List<T> list)
    {
      _list = list;
    }

    public SwitchableReadOnlyList()
    {
      _list = new List<T>();
    }

    public bool IsReadOnly { get; set; }

    public void Add(T item)
    {
      ValidateWriteAccess();
      _list.Add(item);
    }

    public void Clear()
    {
      ValidateWriteAccess();
      _list.Clear();
    }

    public bool Contains(T item)
    {
      return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      _list.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get { return _list.Count; }
    }

    public bool Remove(T item)
    {
      ValidateWriteAccess();
      return _list.Remove(item);
    }

    public IEnumerator GetEnumerator()
    {
      return _list.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _list.GetEnumerator();
    }

    private void ValidateWriteAccess()
    {
      if (IsReadOnly)
      {
        throw new NotSupportedException("This collection is in read-only mode.");
      }
    }
  }
}
