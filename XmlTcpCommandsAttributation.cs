/*************************************************************************
 * XmlTcpCommands.cs
 * Contains attributes for XmlTcpCommands and related classes.
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

using System.ComponentModel;

namespace Zusi_Datenausgabe
{

  /// <summary>
  /// This class provides the XML file structure used to interpret Zusi data types.
  /// </summary>
  [EditorBrowsableAttribute(EditorBrowsableState.Never)]
  public partial class XmlTcpCommands
  {
  }

  /// <summary>
  /// This class provides one single command entry for use in XML command sets.
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public partial class CommandEntry : ICommandEntry
  {
  }
}
