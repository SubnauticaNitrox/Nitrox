using System;
using System.Collections.Generic;
using System.IO;
using NitroxModel.Helper;
using NitroxModel.Server;

namespace NitroxServer.Serialization.World;

public static class WorldManager
{
    public static readonly string SavesFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox", "saves");
    static WorldManager()
    {
        if (Directory.Exists(SavesFolderDir)) return;
        
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
    
    public static List<Listing> GetSaves()
    {
        List<Listing> savesList = new();
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
                
                DateTime fileLastAccessedTime = File.GetLastWriteTime(File.Exists(Path.Combine(folder, $"WorldData.{fileEnding}")) ?
                                                                      // This file is affected by server saving
                                                                      Path.Combine(folder, $"WorldData.{fileEnding}") :
                                                                      // If the above file doesn't exist (server was never ran), use the Version file instead
                                                                      Path.Combine(folder, $"Version.{fileEnding}"));

                savesList.Add(new Listing
                {
                    Name = Path.GetFileName(folder),
                    Version = version,
                    FileLastAccessed = fileLastAccessedTime
                });
                
                // Set the server.cfg name value to the folder name
                if (Path.GetFileName(folder) != serverConfig.SaveName)
                {
                    using (serverConfig.Update(folder))
                    {
                        serverConfig.SaveName = Path.GetFileName(folder);
                    }
                }
                
            }
            catch
            {
                Log.Error($"World \"{folder}\" could not be processed");
            }
        }
        
        // Order listing based on FileLastAccessed time
        savesList.Sort((x, y) => y.FileLastAccessed.CompareTo(x.FileLastAccessed));
        
        return savesList;
    }
    
    public static void CreateEmptySave(string saveFileDirectory)
    {
        Directory.CreateDirectory(saveFileDirectory);
        
        ServerConfig serverConfig = ServerConfig.Load(saveFileDirectory);
        
        string fileEnding = "json";
        if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
        {
            fileEnding = "nitrox";
        }
        File.Create(Path.Combine(saveFileDirectory, $"Version.{fileEnding}")).Close();
    }
    
    public static bool ValidateSave(string saveFileDirectory)
    {
        return Directory.Exists(saveFileDirectory) ||  !File.Exists(Path.Combine(saveFileDirectory, "server.cfg")) || File.Exists(Path.Combine(saveFileDirectory, "Version.json"));
    }

    public static void DeleteSave(string saveFileDirectory)
    {
        try
        {
            //FileSystem.DeleteDirectory(saveFileDirectory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            Log.Info($"Moving world \"{Path.GetFileName(saveFileDirectory)}\" to the recycling bin.");
            //LauncherNotifier.Success($"Successfully moved save \"{Path.GetFileName(saveFileDirectory)}\" to the recycling bin");
        }
        catch (Exception ex)
        {
            //LauncherNotifier.Error("Error: Could not move the selected save to the recycling bin. Try deleting any remaining files manually.");
            Log.Error($"Could not move save \"{Path.GetFileName(saveFileDirectory)}\" to the recycling bin : {ex.GetType()} {ex.Message}");
        }
    }
    public class Listing
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public DateTime FileLastAccessed { get; set; }
    }
}
