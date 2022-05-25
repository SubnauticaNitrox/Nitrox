using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NitroxModel.Helper;
using NitroxModel.Server;

namespace NitroxServer.Serialization.World;

public static class WorldManager
{
    public static readonly string SavesFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves");

    public static string SelectedWorldName { get; set; }

    private static readonly string[] worldFiles = { "BaseData", "EntityData", "PlayerData", "Version", "WorldData" };

    private static readonly List<Listing> savesCache = new();

    static WorldManager()
    {
        Directory.CreateDirectory(SavesFolderDir);
    }

    public static IEnumerable<Listing> GetSaves()
    {
        if (!savesCache.Any())
        {
            foreach (string folder in Directory.EnumerateDirectories(SavesFolderDir))
            {
                if (!ValidateSave(folder))
                {
                    continue;
                }

                Version version;
                ServerConfig serverConfig = ServerConfig.Load(folder);

                string fileEnding = ".json";
                if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
                {
                    fileEnding = ".nitrox";
                }

                using (FileStream stream = new(Path.Combine(folder, "Version" + fileEnding), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    version = new ServerJsonSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version;
                }
                
                savesCache.Add(new Listing
                {
                    WorldName = Path.GetFileName(folder),
                    WorldGamemode = Convert.ToString(serverConfig.GameMode),
                    WorldVersion = $"v{version}",
                    WorldSaveDir = folder
                });
            }
        }
        return savesCache;
    }

    public static void Refresh()
    {
        savesCache.Clear();
        GetSaves();
    }

    public static string CreateEmptySave(string name)
    {
        string saveDir = Path.Combine(SavesFolderDir, name);

        // Check save path for other "My World" files and increment the end value if there is, so as to prevent duplication
        if (Directory.Exists(saveDir))
        {
            int i = 1;
            string newSelectedWorldName = name;
            while (Directory.Exists(saveDir))
            {
                // Add a number to the end of the name
                newSelectedWorldName = name + $" ({i})";
                saveDir = Path.Combine(SavesFolderDir, newSelectedWorldName);
                i++;
            }
            name = newSelectedWorldName;
        }

        Directory.CreateDirectory(saveDir);

        ServerConfig serverConfig = ServerConfig.Load(saveDir);

        string fileEnding = ".json";
        if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            fileEnding = ".nitrox";
        }

        foreach (string file in worldFiles)
        {
            File.Create(Path.Combine(saveDir, file + fileEnding)).Close();
        }

        serverConfig.SaveName = name;

        return saveDir;
    }

    private static bool ValidateSave(string fileName)
    {
        // A save file is valid when it's named "save#", has a "server.cfg" file, and has all of the nested save file names in it
        string saveDir = Path.Combine(SavesFolderDir, fileName);
        string serverConfigFile = Path.Combine(saveDir, "server.cfg");
        if (!Directory.Exists(saveDir) || !File.Exists(serverConfigFile))
        {
            return false;
        }
        foreach (string file in worldFiles)
        {
            if (!File.Exists(Path.Combine(saveDir, Path.ChangeExtension(file, "json"))) && !File.Exists(Path.Combine(saveDir, Path.ChangeExtension(file, "NITROX"))))
            {
                return false;
            }
        }

        return true;
    }

    public class Listing
    {
        public string WorldName { get; set; }
        public string WorldGamemode {  get; set; }
        public string WorldVersion {  get; set; }
        public string WorldSaveDir { get; set; }
    }
}
