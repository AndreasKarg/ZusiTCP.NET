using System.Collections.Generic;

namespace Zusi_Datenausgabe
{
  public class TcpCommandDictionary
  {
    private readonly Dictionary<int, CommandEntry> _commandByID = new Dictionary<int, CommandEntry>();
    private readonly Dictionary<string, int> _idByName = new Dictionary<string, int>();
    private readonly Dictionary<int, string> _nameByID = new Dictionary<int, string>();

    /// <summary>
    /// Create an instance of TcpCommandDictionary and import data from one XmlTcpCommands instance.
    /// </summary>
    /// <param name="xmlTcpCommands">The XmlTcpCommands from where the data is to be imported.</param>
    public TcpCommandDictionary(XmlTcpCommands xmlTcpCommands)
    {
      foreach (var entry in xmlTcpCommands.Command)
      {
        _commandByID.Add(entry.ID, entry);
        _idByName.Add(entry.Name, entry.ID);
        _nameByID.Add(entry.ID, entry.Name);
      }
    }

    /// <summary>
    /// Create an instance of TcpCommandDictionary and import data from an XML file.
    /// </summary>
    /// <param name="xmlFilePath">Path to the XML file where the data is to be imported from.</param>
    public TcpCommandDictionary(string xmlFilePath)
      : this(XmlTcpCommands.LoadFromFile(xmlFilePath))
    {
    }

    /// <summary>
    /// Create an empty instance of TcpCommandDictionary.
    /// </summary>
    public TcpCommandDictionary()
    {
    }

    /// <summary>
    /// Contains a list of Zusi commands accessible by their numeric id.
    /// </summary>
    public IReadOnlyDictionary<int, CommandEntry> CommandByID
    {
      get { return new ReadOnlyDictionary<int, CommandEntry>(_commandByID); }
    }

    /// <summary>
    /// Contains a list of numeric Zusi command IDs accessible by their name
    /// (Taken from the TCP Server's commandset.ini. See commandset.xml for
    /// an adaption for this library.)
    /// </summary>
    public IReadOnlyDictionary<string, int> IDByName
    {
      get { return new ReadOnlyDictionary<string, int>(_idByName); }
    }

    /// <summary>
    /// Contains a list of Zusi command names accessible by their numeric ID.
    /// </summary>
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
  }
}