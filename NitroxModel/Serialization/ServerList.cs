using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NitroxModel.Helper;

namespace NitroxModel.Serialization;

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
            list.Add(new Entry("Your server", "127.0.0.1", DEFAULT_PORT));
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
        if (!File.Exists(file))
        {
            instance = Default;
            instance.Save();
            return instance;
        }

        ServerList list = new();
        foreach (string line in File.ReadAllLines(file))
        {
            Entry entry = Entry.FromLine(line);
            if (entry == null)
            {
                continue;
            }

            list.entries.Add(entry);
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

        public static Entry FromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }
            string[] parts = line.Split('|');
            int port;
            string address;
            switch (parts.Length)
            {
                case 2:
                    // Split from address as format "hostname:port".
                    string[] addressSplit = parts[1].Split(':');
                    address = addressSplit[0];
                    if (!int.TryParse(addressSplit.ElementAtOrDefault(1), out port))
                    {
                        port = DEFAULT_PORT;
                    }
                    break;
                case 3:
                    address = parts[1].Trim();
                    if (!int.TryParse(parts[2], out port))
                    {
                        port = DEFAULT_PORT;
                    }
                    break;
                default:
                    throw new Exception($"Expected server entry to have 2 or 3 parts: {line}");
            }

            string name = parts[0].Trim();
            return new Entry(name, address, port);
        }

        public override string ToString()
        {
            return $"{Name}|{Address}|{Port}";
        }
    }
}
