/*************************************************************************
 * TCPCommands.cs
 * Contains the TCPCommands class.
 *
 * (C) 2009-2012 Andreas Karg, <Clonkman@gmx.de>
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Zusi_Datenausgabe
{

  /// <summary>
  /// This class provides the XML file structure used to interpret Zusi data types.
  /// </summary>
  [EditorBrowsableAttribute(EditorBrowsableState.Never)]
  public partial class TCPCommands
  {
    private Dictionary<int, CommandEntry> _commandByID = new Dictionary<int, CommandEntry>();
    private Dictionary<string, int> _idByName = new Dictionary<string, int>();
    private Dictionary<int, string> _nameByID = new Dictionary<int, string>();

    /// <summary>
    /// Contains a list of Zusi commands accessible by their numeric id.
    /// </summary>
    [XmlIgnore]
    public IReadOnlyDictionary<int, CommandEntry> CommandByID
    {
      get { return new ReadOnlyDictionary<int, CommandEntry>(_commandByID); }
    }

    /// <summary>
    /// Contains a list of numeric Zusi command IDs accessible by their name
    /// (Taken from the TCP Server's commandset.ini. See commandset.xml for
    /// an adaption for this library.)
    /// </summary>
    [XmlIgnore]
    public IReadOnlyDictionary<string, int> IDByName
    {
      get { return new ReadOnlyDictionary<string, int>(_idByName); }
    }

    /// <summary>
    /// Contains a list of Zusi command names accessible by their numeric ID.
    /// </summary>
    [XmlIgnore]
    public IReadOnlyDictionary<int, string> NameByID
    {
      get { return new ReadOnlyDictionary<int, string>(_nameByID); }
    }

    /// <summary>
    /// Identical to this.<see cref="CommandByID"/>.
    /// </summary>
    /// <param name="index">Contains the command's ID.</param>
    public CommandEntry this[int index]
    {
      get
      {
        return CommandByID[index];
      }
    }

    private void InitializeDictionaries()
    {
      foreach (var entry in commandField)
      {
        _commandByID.Add(entry.ID, entry);
        _idByName.Add(entry.Name, entry.ID);
        _nameByID.Add(entry.ID, entry.Name);
      }
    }

    /// <summary>
    /// Load XML data from a file and create a TCPCommands instance from it.
    /// </summary>
    /// <param name="filePath">Contains the path to the XML file</param>
    /// <returns>A new TCPCommands instance with data.</returns>
    public static TCPCommands LoadFromFile(String filePath)
    {
      TCPCommands tempResult = TCPCommands.LoadFromFileInternal(filePath);

      tempResult.InitializeDictionaries();

      return tempResult;
    }
  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public partial class CommandEntry
  {
  }
}
