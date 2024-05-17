using System.Diagnostics;
using System.IO;
using NitroxModel.Helper;
using NitroxServer_Subnautica.Resources.Parsers;

namespace NitroxServer_Subnautica.Resources;

public static class ResourceAssetsParser
{
    private static ResourceAssets resourceAssets;

    public static ResourceAssets Parse()
    {
        if (resourceAssets is not null)
        {
            return resourceAssets;
        }

        using PrefabPlaceholderGroupsParser prefabPlaceholderGroupsParser = new();

        Stopwatch stopwatch = new();

        Log.Debug("Parsing world entity...");
        stopwatch.Restart();
        System.Collections.Generic.Dictionary<string, UWE.WorldEntityInfo> worldEntity = new WorldEntityInfoParser().ParseFile();
        Log.Debug($"Done in {stopwatch.ElapsedMilliseconds}ms.");

        Log.Debug("Parsing random start generator...");
        stopwatch.Restart();
        NitroxModel.DataStructures.GameLogic.RandomStartGenerator random = new RandomStartParser().ParseFile();
        Log.Debug($"Done in {stopwatch.ElapsedMilliseconds}ms.");

        Log.Debug("Parsing entity distributions...");
        stopwatch.Restart();
        string entityDistribution = new EntityDistributionsParser().ParseFile();
        Log.Debug($"Done in {stopwatch.ElapsedMilliseconds}ms.");

        Log.Debug("Parsing prefab placeholders...");
        stopwatch.Restart();
        System.Collections.Generic.Dictionary<string, NitroxServer.Resources.PrefabPlaceholdersGroupAsset> prefabPlaceholdersGroupsByGroupClassId = prefabPlaceholderGroupsParser.ParseFile();
        Log.Debug($"Done in {stopwatch.ElapsedMilliseconds}ms.");

        stopwatch.Reset();

        resourceAssets = new ResourceAssets
        {
            WorldEntitiesByClassId = new WorldEntityInfoParser().ParseFile(),
            LootDistributionsJson = new EntityDistributionsParser().ParseFile(),
            PrefabPlaceholdersGroupsByGroupClassId = prefabPlaceholderGroupsParser.ParseFile(),
            NitroxRandom = new RandomStartParser().ParseFile()
        };
        
        ResourceAssets.ValidateMembers(resourceAssets);
        return resourceAssets;
    }

    public static string FindDirectoryContainingResourceAssets()
    {
        string subnauticaPath = NitroxUser.GamePath;
        if (string.IsNullOrEmpty(subnauticaPath))
        {
            throw new DirectoryNotFoundException("Could not locate Subnautica installation directory for resource parsing");
        }

        if (File.Exists(Path.Combine(subnauticaPath, "Subnautica_Data", "resources.assets")))
        {
            return Path.Combine(subnauticaPath, "Subnautica_Data");
        }
        if (File.Exists(Path.Combine("..", "resources.assets"))) //  SubServer => Subnautica/Subnautica_Data/SubServer
        {
            return Path.GetFullPath(Path.Combine(".."));
        }
        if (File.Exists(Path.Combine("..", "Subnautica_Data", "resources.assets"))) //  SubServer => Subnautica/SubServer
        {
            return Path.GetFullPath(Path.Combine("..", "Subnautica_Data"));
        }
        if (File.Exists("resources.assets")) //  SubServer/* => Subnautica/Subnautica_Data/
        {
            return Directory.GetCurrentDirectory();
        }
        throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
    }
}
