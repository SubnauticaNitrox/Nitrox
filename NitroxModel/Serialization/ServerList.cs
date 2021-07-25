using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NitroxModel.Helper;
using NitroxModel.OS;

namespace NitroxModel.Serialization
{
    public class ServerList
    {
        private const string SERVERS_FILE_NAME = "servers";
        private static ServerList instance;
        private readonly List<Entry> entries = new();
        public static ServerList Instance => instance ??= From(DefaultFile);

        private static ServerList Default
        {
            get
            {
                ServerList list = new();
                list.Add(new Entry("local server", "127.0.0.1", 11000));
                return list;
            }
        }

        public static string DefaultFile => Path.Combine(NitroxAppData.Instance.LauncherPath, SERVERS_FILE_NAME);

        public IEnumerable<Entry> Entries => entries;

        public static ServerList From(string file = null)
        {
            // TODO: Remove backward compatibility after 1.5.0.0 release (no need to move old server file).
            FileSystem.Instance.ReplaceFile(SERVERS_FILE_NAME, DefaultFile);

            // Create file if it doesn't exist yet.
            file ??= DefaultFile;
            if (!File.Exists(file))
            {
                instance = Default;
                instance.Save(file);
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

        public void Save(string file = null)
        {
            file ??= DefaultFile;

            using StreamWriter writer = new(new FileStream(file, FileMode.Create, FileAccess.Write));
            foreach (Entry entry in entries)
            {
                writer.WriteLine(entry.ToString());
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

            public Entry(string name, string address, int port)
            {
                Validate.String(name);
                Validate.NotNull(address);
                Validate.IsTrue(port is >= 1024 and <= ushort.MaxValue);

                Name = name.Trim();
                Address = address.Trim();
                Port = port;
            }

            public Entry(string name, IPAddress address, int port) : this(name, address.ToString(), port)
            {
            }

            public Entry(string name, string address, string port) : this(name, address, int.Parse(port))
            {
            }

            public static Entry FromLine(string line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    return null;
                }
                string[] parts = line.Split('|');
                if (parts.Length != 3)
                {
                    throw new Exception($"Expected server entry to have 3 parts: {line}");
                }
                
                return new Entry(parts[0].Trim(), parts[1].Trim(), int.Parse(parts[2].Trim()));
            }

            public override string ToString()
            {
                return $"{Name}|{Address}|{Port}";
            }
        }
    }
}
