/*************************************************************************
 * XmlTcpCommands.cs
 * Contains the XmlTcpCommands class.
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
  public partial class XmlTcpCommands
  {
    /// <summary>
    /// Load XML data from a file and create a XmlTcpCommands instance from it.
    /// </summary>
    /// <param name="filePath">Contains the path to the XML file</param>
    /// <returns>A new XmlTcpCommands instance with data.</returns>
    public static XmlTcpCommands LoadFromFile(String filePath)
    {
      XmlTcpCommands tempResult = XmlTcpCommands.LoadFromFileInternal(filePath);

      return tempResult;
    }
  }

  [EditorBrowsable(EditorBrowsableState.Never)]
  public partial class CommandEntry
  {
  }
}
