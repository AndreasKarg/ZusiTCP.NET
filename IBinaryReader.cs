#region header

// /*************************************************************************
//  * ISourcedBinaryReader.cs
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

using System;

#endregion

namespace Zusi_Datenausgabe
{
  public interface IBinaryReader : IDisposable
  {
    void Close();
    int PeekChar();
    int Read(byte[] buffer, int index, int count);
    int Read(char[] buffer, int index, int count);
    int Read();
    bool ReadBoolean();
    byte ReadByte();
    byte[] ReadBytes(int count);
    char ReadChar();
    char[] ReadChars(int count);
    decimal ReadDecimal();
    double ReadDouble();
    short ReadInt16();
    int ReadInt32();
    long ReadInt64();
    sbyte ReadSByte();
    float ReadSingle();
    string ReadString();
    ushort ReadUInt16();
    uint ReadUInt32();
    ulong ReadUInt64();
  }
}
