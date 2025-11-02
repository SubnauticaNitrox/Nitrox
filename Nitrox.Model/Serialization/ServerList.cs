using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nitrox.Model.Helper;

namespace Nitrox.Model.Serialization;

public class ServerList
{
    private const string SERVERS_FILE_NAME = "servers";
    public const int DEFAULT_PORT = 11000;
    private static ServerList instance;
    private readonly List<Entry> entries = new();
    public static ServerList Instance => instance ??= Refresh();

    private static ServerList Default
    {
        get
        {
            ServerList list = new();
            list.Add(new Entry("Your server", IPAddress.Loopback.ToString(), DEFAULT_PORT));
            return list;
        }
    }

    public static string DefaultFile => Path.Combine(NitroxUser.AppDataPath, SERVERS_FILE_NAME);

    public IEnumerable<Entry> Entries => entries;

    public static ServerList Refresh()
    {
        return instance = From();
    }

    public static ServerList From()
    {
        // Create file if it doesn't exist yet.
        string file = DefaultFile;
        string[] lines;
        try
        {
            lines = File.ReadAllLines(file);
        }
        catch (IOException)
        {
            instance = Default;
            instance.Save();
            return instance;
        }

        ServerList list = new();
        foreach (string line in lines)
        {
            if (Entry.FromLine(line) is { } entry)
            {
                list.entries.Add(entry);
            }
        }
        return list;
    }

    public void Add(Entry entry)
    {
        entries.Add(entry);
    }

    public void Save()
    {
        string file = DefaultFile;

        using StreamWriter writer = new(new FileStream(file, FileMode.Create, FileAccess.Write));
        foreach (Entry entry in entries)
        {
            if (entry.Persist)
            {
                writer.WriteLine(entry.ToString());
            }
        }
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= entries.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        entries.RemoveAt(index);
    }

    public class Entry
    {
        public string Name { get; }
        public string Address { get; }
        public int Port { get; }

        /// <summary>
        ///     If true, entry will be saved to storage.
        /// </summary>
        public bool Persist { get; }

        public Entry(string name, string address, int port, bool persist = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name in ServerList.Entry constructor can't be null or whitespace");
            }
            Validate.NotNull(address);

            Name = name.Trim();
            Address = address.Trim();
            Port = port;
            Persist = persist;
        }

        public Entry(string name, IPAddress address, int port, bool persist = true) : this(name, address.ToString(), port, persist)
        {
        }

        public static Entry? FromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }
            string[] parts = line.Split('|');
            int port = DEFAULT_PORT;
            string address;
            switch (parts.Length)
            {
                case 2:
                    address = ParseAddressWithOptionalPort(parts[1], out port);
                    break;
                case 3:
                    address = parts[1].Trim();
                    if (!int.TryParse(parts[2], out port))
                    {
                        port = DEFAULT_PORT;
                    }
                    break;
                default:
                    Log.Warn($"A server list entry contained too many elements. Either 2 or 3 is valid but was: {line}");
                    return null;
            }

            string name = parts[0].Trim();
            return new Entry(name, address, port);

            static string ParseAddressWithOptionalPort(string value, out int parsedPort)
            {
                string trimmed = value.Trim();
                parsedPort = DEFAULT_PORT;

                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.Contains(']'))
                {
                    int closingBracket = trimmed.LastIndexOf(']');
                    if (closingBracket > 0)
                    {
                        string potentialPort = trimmed[(closingBracket + 1)..].TrimStart(':');
                        if (!string.IsNullOrWhiteSpace(potentialPort))
                        {
                            int.TryParse(potentialPort, out parsedPort);
                        }

                        return trimmed.Substring(1, closingBracket - 1);
                    }
                }

                int lastColonIndex = trimmed.LastIndexOf(':');
                if (lastColonIndex > -1 && lastColonIndex < trimmed.Length - 1)
                {
                    string potentialPort = trimmed[(lastColonIndex + 1)..];
                    if (int.TryParse(potentialPort, out int portCandidate))
                    {
                        parsedPort = portCandidate;
                        return trimmed[..lastColonIndex];
                    }
                }

                return trimmed;
            }
        }

        public override string ToString()
        {
            return $"{Name}|{Address}|{Port}";
        }
    }
}
