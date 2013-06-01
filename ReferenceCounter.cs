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

using System.Collections.Generic;
using System.Diagnostics;

namespace Zusi_Datenausgabe
{
  public class ReferenceCounter<T>
  {
    private Dictionary<T, int> _referenceCounts = new Dictionary<T, int>();

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

    private void ReleaseItem(T i)
    {
      var val = _referenceCounts[i];

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
      _referenceCounts[item]++;
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