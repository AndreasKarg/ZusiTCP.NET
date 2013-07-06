using System;
using System.Collections.Generic;
using System.Linq;
using Zusi_Datenausgabe.ReadOnlyDictionary;

namespace Zusi_Datenausgabe
{
  public interface ITcpCommandDictionary
  {
    /// <summary>
    /// Contains a list of Zusi commands accessible by their numeric id.
    /// </summary>
    IReadOnlyDictionary<int, ICommandEntry> CommandByID { get; }

    /// <summary>
    /// Contains a list of numeric Zusi command IDs accessible by their name
    /// (Taken from the TCP Server's commandset.ini. See commandset.xml for
    /// an adaption for this library.)
    /// </summary>
    IReadOnlyDictionary<string, int> IDByName { get; }

    /// <summary>
    /// Contains a list of Zusi command names accessible by their numeric ID.
    /// </summary>
    IReadOnlyDictionary<int, string> NameByID { get; }

    /// <summary>
    /// Identical to this.<see cref="CommandByID"/>.
    /// </summary>
    /// <param name="index">Contains the command's ID.</param>
    ICommandEntry this[int index] { get; }
  }

  public class TcpCommandDictionary : ITcpCommandDictionary
  {
    private readonly IDictionary<int, ICommandEntry> _commandByID;
    private readonly IDictionary<string, int> _idByName;
    private readonly IDictionary<int, string> _nameByID;

    /// <summary>
    /// Create an instance of TcpCommandDictionary and import data from one XmlTcpCommands instance.
    /// </summary>
    /// <param name="xmlTcpCommands">The XmlTcpCommands from where the data is to be imported.</param>
    public TcpCommandDictionary(XmlTcpCommands xmlTcpCommands, IDictionary<int, ICommandEntry> commandByID, IDictionary<string, int> idByName, IDictionary<int, string> nameByID)
    {
      _commandByID = commandByID;
      _idByName = idByName;
      _nameByID = nameByID;
      Import(xmlTcpCommands);
    }

    /// <summary>
    /// Create an instance of TcpCommandDictionary and import data from an XML file.
    /// </summary>
    /// <param name="xmlFilePath">Path to the XML file where the data is to be imported from.</param>
    public TcpCommandDictionary(string xmlFilePath, IDictionary<int, ICommandEntry> commandByID, IDictionary<string, int> idByName, IDictionary<int, string> nameByID)
      : this(XmlTcpCommands.LoadFromFile(xmlFilePath), commandByID, idByName, nameByID)
    {
    }

    /// <summary>
    /// Create an empty instance of TcpCommandDictionary.
    /// </summary>
    public TcpCommandDictionary(IDictionary<int, ICommandEntry> commandByID, IDictionary<string, int> idByName, IDictionary<int, string> nameByID)
    {
      _commandByID = commandByID;
      _idByName = idByName;
      _nameByID = nameByID;
    }

    /// <summary>
    /// Contains a list of Zusi commands accessible by their numeric id.
    /// </summary>
    public IReadOnlyDictionary<int, ICommandEntry> CommandByID
    {
      get { return new ReadOnlyDictionary<int, ICommandEntry>(_commandByID); }
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
    public ICommandEntry this[int index]
    {
      get
      {
        return CommandByID[index];
      }
    }

    public void Import(XmlTcpCommands source)
    {
      Import(source.Command.Cast<ICommandEntry>());
    }

    public void Import(ITcpCommandDictionary source)
    {
      Import(source.CommandByID.Values);
    }

    private void Import(IEnumerable<ICommandEntry> source)
    {
      foreach (var cmd in source)
      {
        ImportEntry(cmd);
      }
    }

    private void ImportEntry(ICommandEntry entry)
    {
      try
      {
        _commandByID.Add(entry.ID, entry);
        _idByName.Add(entry.Name, entry.ID);
        _nameByID.Add(entry.ID, entry.Name);
      }
      catch (ArgumentException ex)
      {
        var message = String.Format("Source already contains command entry for {1} ({0})", entry.Name,
          entry.ID);

        throw new ArgumentException(message, "entry", ex);
      }
    }
  }
}