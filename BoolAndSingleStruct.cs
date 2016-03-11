/*************************************************************************
 * Zusi-Datenausgabe.cs
 * Contains main logic for the TCP interface.
 *
 * (C) 2009-2011 Andreas Karg, <Clonkman@gmx.de>
 *
 * This file is part of Zusi TCP Interface.NET.
 *
 * Zusi TCP Interface.NET is free software: you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * Zusi TCP Interface.NET is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Zusi TCP Interface.NET.
 * If not, see <http://www.gnu.org/licenses/>.
 *
 *************************************************************************/

namespace Zusi_Datenausgabe
{
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public struct BoolAndSingleStruct
  {
    public BoolAndSingleStruct(int lng, bool retVal, float pz80Val) : this()
    {
      ExtractedLength = lng;
      ExtractedData = retVal;
      PZ80Data = pz80Val;
    }

    ///<summary>The length, that was neccessary to extract the data.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedLengthAttribute()]
    public int ExtractedLength {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedDataAttribute()]
    public bool ExtractedData {private set; get;}

    ///<summary>The data, that was extracted.</summary>
    [Zusi_Datenausgabe.Compyling.ExtractedDataAttribute()]
    public float PZ80Data {private set; get;}
  }
}
