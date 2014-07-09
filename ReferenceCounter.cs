#region header

// /*************************************************************************
//  * RequestetDataClerk.cs
//  * Contains main logic for the TCP interface.
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

#region Using

using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Zusi_Datenausgabe
{
  public class ReferenceCounter<T>
  {
    private readonly Dictionary<T, int> _referenceCounts = new Dictionary<T, int>();

    public void ClaimRange(IEnumerable<T> range)
    {
      foreach (var i in range)
      {
        ClaimItem(i);
      }
    }

    public void ReleaseRange(IEnumerable<T> range)
    {
      foreach (var i in range)
      {
        ReleaseItem(i);
      }
    }

    public bool ExistAll(IEnumerable<T> range)
    {
      foreach (var i in range)
      {
        if (!ExistItem(i)) return false;
      }
      return true;
    }


    private void ReleaseItem(T i)
    {
      int val = _referenceCounts[i];

      Debug.Assert(val > 0);

      if (val == 1)
      {
        _referenceCounts.Remove(i);
      }
      else
      {
        _referenceCounts[i] = val - 1;
      }
    }

    public void ClaimItem(T item)
    {
      if (!_referenceCounts.ContainsKey(item)) _referenceCounts.Add(item, 0);
      _referenceCounts[item]++;
    }

    public bool ExistItem(T item)
    {
      if (!_referenceCounts.ContainsKey(item)) return false;
      return _referenceCounts[item] > 0;
    }

    public void Clear()
    {
      _referenceCounts.Clear();
    }

    public IEnumerable<T> ReferencedToIEnumerable()
    {
      return new List<T>(_referenceCounts.Keys);
    }
  }
}
