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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Zusi_Datenausgabe;

namespace TCPCommandset
{
    public partial class TCPCommands
    {
        [XmlIgnore]
        public ZusiData<int, CommandEntry> CommandByID { get; private set; }

        [XmlIgnore]
        public ZusiData<string, int> IDByName { get; private set; }

        [XmlIgnore]
        public ZusiData<int, string> NameByID { get; private set; }

        public CommandEntry this[int index]
        {
            get { return CommandByID[index]; }
        }

        private void InitializeDictionaries()
        {
            var tmpCommandByID = new Dictionary<int, CommandEntry>();
            var tmpIDByName = new Dictionary<string, int>();
            var tmpNameByID = new Dictionary<int, string>();

            foreach (var entry in commandField)
            {
                tmpCommandByID.Add(entry.ID, entry);
                tmpIDByName.Add(entry.Name, entry.ID);
                tmpNameByID.Add(entry.ID, entry.Name);
            }

            CommandByID = new ZusiData<int, CommandEntry>(tmpCommandByID);
            IDByName = new ZusiData<string, int>(tmpIDByName);
            NameByID = new ZusiData<int, string>(tmpNameByID);
        }

        public static TCPCommands LoadFromFile(String filePath)
        {
            TCPCommands tempResult = TCPCommands.LoadFromFileInternal(filePath);

            tempResult.InitializeDictionaries();

            return tempResult;
        }
    }
}
