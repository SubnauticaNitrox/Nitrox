using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Server;

namespace NitroxServer.Serialization.World;

public static class WorldManager
{
    public static readonly string SavesFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves");

    public static readonly string ErrorLogName = "ErrorLog.txt";

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
            Log.Error(ex, "Couldn't create \"saves\" folder");
            throw new Exception(ex.ToString());
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
                // Don't add the file to the list if it doesn't validate
                if (!ValidateSave(folder))
                {
                    continue;
                }

                Version version;
                ServerConfig serverConfig = ServerConfig.Load(folder);

                string fileEnding = "json";
                if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
                {
                    fileEnding = "nitrox";
                }

                using (FileStream stream = new(Path.Combine(folder, $"Version.{fileEnding}"), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    version = new ServerJsonSerializer().Deserialize<SaveFileVersion>(stream)?.Version ?? NitroxEnvironment.Version;
                }

                DateTime fileLastAccessedTime;
                if (File.Exists(Path.Combine(folder, $"WorldData.{fileEnding}")))
                {
                    fileLastAccessedTime = File.GetLastWriteTime(Path.Combine(folder, $"WorldData.{fileEnding}"));
                }
                else
                {
                    fileLastAccessedTime = File.GetLastWriteTime(Path.Combine(folder, $"Version.{fileEnding}")); // This file was created when the save was created, so it can be used as the backup to get this write time if this is a new save (the WorldData file wouldn't exist)
                }

                // Change the paramaters here to define what save file versions are eligible for use/upgrade
                bool isValidVersion = true;
                if (version < new Version(1, 6, 0, 1) || version > NitroxEnvironment.Version)
                {
                    isValidVersion = false;
                }

                savesCache.Add(new Listing
                {
                    WorldName = Path.GetFileName(folder),
                    WorldGamemode = Convert.ToString(serverConfig.GameMode),
                    WorldVersion = $"v{version}",
                    WorldSaveDir = folder,
                    IsValidSave = isValidVersion,
                    FileLastAccessed = fileLastAccessedTime
                });

                // Set the server.cfg name value to the folder name
                if (Path.GetFileName(folder) != serverConfig.SaveName)
                {
                    serverConfig.Update(folder, c => { c.SaveName = Path.GetFileName(folder); });
                }

            }
            catch
            {
                Log.Error($"World \"{folder}\" could not be processed");
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

        string fileEnding = "json";
        if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            fileEnding = "nitrox";
        }
        File.Create(Path.Combine(saveDir, $"Version.{fileEnding}")).Close();

        serverConfig.SaveName = name;

        return saveDir;
    }

    public static bool ValidateSave(string saveFileDirectory, bool isImporting = false)
    {
        if (!Directory.Exists(saveFileDirectory))
        {
            return false;
        }

        // A save file is valid when it has a server.cfg file in it (if not importing a file) and if it has at least a Version.(ext) save file in it
        if (isImporting && !File.Exists(Path.Combine(saveFileDirectory, "server.cfg")))
        {
            return false;
        }

        if (!File.Exists(Path.Combine(saveFileDirectory, $"Version.json")))
        {
            return false;
        }

        return true;
    }

    public static void BackupSave(string saveFileDirectory, bool IsCorrupted = false)
    {
        if (!Directory.Exists(Path.Combine(saveFileDirectory, "Backups")))
        {
            Directory.CreateDirectory(Path.Combine(saveFileDirectory, "Backups"));
        }

        string outZip = Path.Combine(saveFileDirectory, "Backups", $"{Path.GetFileName(saveFileDirectory)} Backup - {DateTime.Now:yyyy MMM dd, HHmm ss}");
        if (IsCorrupted)
        {
            outZip = Path.Combine(saveFileDirectory, "Backups", $"(CORRUPTED) {Path.GetFileName(saveFileDirectory)} Backup - {DateTime.Now:yyyy MMM dd, HHmmss}");
            Log.WarnSensitive("Creating a backup of the corrupted save files at {path}", $"{outZip}.zip");
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
            File.Copy(Path.Combine(saveFileDirectory, ErrorLogName), Path.Combine(outZip, ErrorLogName));
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

            // A backup is added to the list if the compressed folder contains all the valid
            // save files, doesn't contain an error log, and contains the name of the world.
            foreach (string file in Directory.EnumerateFiles(Path.Combine(saveFileDirectory, "Backups")))
            {
                FileInfo fileInfo = new(file);

                if (fileInfo.Extension != ".zip") { continue; }

                DateTime? backupWriteTime = null;
                bool isValid = true;
                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name == ErrorLogName || !WorldFiles.Contains(entry.Name))
                        {
                            isValid = false;
                            break;
                        }

                        if (!backupWriteTime.HasValue)
                        {
                            backupWriteTime = entry.LastWriteTime.DateTime;
                        }
                    }
                }

                if (!isValid || !fileInfo.Name.Contains($"{Path.GetFileName(saveFileDirectory)} Backup")) { continue; }

                backupsCache.Add(new Listing
                {
                    WorldName = $"Backup {(DateTime)backupWriteTime}",
                    WorldSaveDir = file,
                    FileLastAccessed = (DateTime)backupWriteTime
                });
            }

            backupsCache.Sort((x, y) => y.FileLastAccessed.CompareTo(x.FileLastAccessed));

            int maxBackups = 5;
            if (backupsCache.Count > maxBackups)
            {
                for (int i = 1; i < backupsCache.Count; i++)
                {
                    if (i > maxBackups-1)
                    {
                        File.Delete(backupsCache.ElementAt(i).WorldSaveDir);
                    }
                }

                backupsCache.RemoveRange(3, backupsCache.Count - maxBackups);
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
