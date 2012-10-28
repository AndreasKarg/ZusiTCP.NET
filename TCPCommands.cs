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
