using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;

namespace NitroxServer.Serialization.World;

public static class WorldManager
{
    public static readonly string SavesFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves");

    public static readonly string[] WorldFiles = { "BaseData.json", "EntityData.json", "PlayerData.json", "Version.json", "WorldData.json" };

    private static readonly List<Listing> savesCache = new();
    private static readonly List<Listing> backupsCache = new();

    static WorldManager()
    {
        try
        {
            Directory.CreateDirectory(SavesFolderDir);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }

    public static IEnumerable<Listing> GetSaves()
    {
        if (savesCache.Count != 0)
        {
            return savesCache;
        }

        foreach (string folder in Directory.EnumerateDirectories(SavesFolderDir))
        {
            try
            {
                // Don't add the file to the list if it doesn't validate or contain a "server.cfg" file
                string serverConfigFile = Path.Combine(folder, "server.cfg");
                if (!ValidateSave(folder) || !File.Exists(serverConfigFile))
                {
                    continue;
                }

                Version version;
                ServerConfig serverConfig = ServerConfig.Load(folder);

                using (FileStream stream = new(Path.Combine(folder, "Version.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    version = new ServerJsonSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version;
                }

                // Change the paramaters here to define what save file versions are eligible for use/upgrade
                bool IsValidVersion = true;
                if (version < new Version(1, 6, 0, 0) || version > NitroxEnvironment.Version)
                {
                    IsValidVersion = false;
                }

                savesCache.Add(new Listing
                {
                    WorldName = Path.GetFileName(folder),
                    WorldGamemode = Convert.ToString(serverConfig.GameMode),
                    WorldVersion = $"v{version}",
                    WorldSaveDir = folder,
                    IsValidSave = IsValidVersion,
                    FileLastAccessed = File.GetLastWriteTime(Path.Combine(folder, $"WorldData.json"))
                });

                // Set the server.cfg name value to the folder name
                if (Path.GetFileName(folder) != serverConfig.SaveName)
                {
                    serverConfig.Update(folder, c => { c.SaveName = Path.GetFileName(folder); });
                }

            }
            catch
            {
                Log.Error($"World could not be processed");
            }
        }
        // Order listing based on FileLastAccessed time
        savesCache.Sort((x, y) => y.FileLastAccessed.CompareTo(x.FileLastAccessed));

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

        foreach (string file in WorldFiles)
        {
            File.Create(Path.Combine(saveDir, file)).Close();
        }

        serverConfig.SaveName = name;

        return saveDir;
    }

    public static bool ValidateSave(string saveFileDirectory)
    {
        // A save file is valid when it has all of the nested save file names in it
        if (!Directory.Exists(saveFileDirectory))
        {
            return false;
        }
        foreach (string file in WorldFiles)
        {
            if (!File.Exists(Path.Combine(saveFileDirectory, file)))
            {
                return false;
            }
        }

        return true;
    }

    public static void BackupSave(string saveFileDirectory, bool IsCorrupted = false)
    {
        if (!Directory.Exists(Path.Combine(saveFileDirectory, "Backups")))
        {
            Directory.CreateDirectory(Path.Combine(saveFileDirectory, "Backups"));
        }

        string outZip = Path.Combine(saveFileDirectory, "Backups", $"{Path.GetFileName(saveFileDirectory)} Backup - {DateTime.Now:yyyy MMM dd, HHmmss}");
        if (IsCorrupted)
        {
            outZip = Path.Combine(saveFileDirectory, "Backups", $"(CORRUPTED) {Path.GetFileName(saveFileDirectory)} Backup - {DateTime.Now:yyyy MMM dd, HHmmss}");
            Log.InfoSensitive("Creating a backup of the corrupted save files at {path}", $"{outZip}.zip");
        }
        else
        {
            Log.InfoSensitive("Creating a backup of the save files at {path}", $"{outZip}.zip");
        }
        Directory.CreateDirectory(outZip);

        foreach (string file in WorldFiles)
        {
            File.Copy(Path.Combine(saveFileDirectory, file), Path.Combine(outZip, file));
        }

        if (IsCorrupted)
        {
            File.Copy(Path.Combine(saveFileDirectory, "ErrorLog.txt"), Path.Combine(outZip, "ErrorLog.txt"));
        }

        FileSystem.Instance.ZipFilesInDirectory(outZip, $"{outZip}.zip");
        Directory.Delete(outZip, true);

        // Check Backups folder and delete 4th oldest backup, if any exists (except for any that have "(CORRUPTED)" in the name)
        GetBackups(saveFileDirectory);
        
    }

    public static IEnumerable<Listing> GetBackups(string saveFileDirectory)
    {
        if (!backupsCache.Any())
        {
            if (!Directory.Exists(Path.Combine(saveFileDirectory, "Backups")))
            {
                return Enumerable.Empty<Listing>();
            }

            foreach (string file in Directory.EnumerateFiles(Path.Combine(saveFileDirectory, "Backups")))
            {
                FileInfo fileInfo = new(file);
                if (!fileInfo.Name.Contains("(CORRUPTED)") || fileInfo.Name.Contains($"{Path.GetFileName(saveFileDirectory)} Backup") || fileInfo.Extension == ".zip")
                {
                    backupsCache.Add(new Listing
                    {
                        WorldName = $"Backup {File.GetLastWriteTime(file)}",
                        WorldSaveDir = file,
                        FileLastAccessed = File.GetLastWriteTime(file)
                });
                }
            }

            backupsCache.Sort((x, y) => y.FileLastAccessed.CompareTo(x.FileLastAccessed));

            if (backupsCache.Count > 3)
            {
                for (int i = 1; i < backupsCache.Count; i++)
                {
                    if (i > 2)
                    {
                        File.Delete(backupsCache.ElementAt(i).WorldSaveDir);
                    }
                }

                backupsCache.RemoveRange(3, backupsCache.Count - 3);
            }

        }
        
        return backupsCache;
    }

    public static void RefreshBackups(string saveFileDirectory)
    {
        backupsCache.Clear();
        GetBackups(saveFileDirectory);
    }

    public class Listing
    {
        public string WorldName { get; set; }
        public string WorldGamemode {  get; set; }
        public string WorldVersion {  get; set; }
        public string WorldSaveDir { get; set; }
        public bool IsValidSave { get; set; }
        public DateTime FileLastAccessed { get; set; }
    }
}
