using System.IO;
using NitroxModel.Helper;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer_Subnautica.Resources.Parsers;

namespace NitroxServer_Subnautica.Resources;

public static class ResourceAssetsParser
{
    private static ResourceAssets resourceAssets;

    public static ResourceAssets Parse(SpawnMode spawnMode)
    {
        if (resourceAssets == null)
        {
            // when we are in baked mode, we already have all of the entities spawned. there is no need
            // to do expensive parsing of spawning related resources. 
            resourceAssets = (spawnMode == SpawnMode.BAKED) ? ParseWithoutSpawnData() : ParseAllData();

            AssetParser.Dispose();
        }

        return resourceAssets;
    }

    private static ResourceAssets ParseAllData()
    {
        ResourceAssets resourceAssets;

        using (PrefabPlaceholderGroupsParser prefabPlaceholderGroupsParser = new())
        {
            resourceAssets = new ResourceAssets
            {
                WorldEntitiesByClassId = new WorldEntityInfoParser().ParseFile(),
                LootDistributionsJson = new EntityDistributionsParser().ParseFile(),
                PrefabPlaceholderGroupsByGroupClassId = prefabPlaceholderGroupsParser.ParseFile(),
                NitroxRandom = new RandomStartParser().ParseFile()
            };
        }

        ResourceAssets.ValidateMembers(resourceAssets);

        return resourceAssets;
    }

    private static ResourceAssets ParseWithoutSpawnData()
    {
        ResourceAssets resourceAssets;

        resourceAssets = new ResourceAssets
        {
            NitroxRandom = new RandomStartParser().ParseFile()
        };

        Validate.NotNull(resourceAssets.NitroxRandom);

        return resourceAssets;
    }

    public static string FindDirectoryContainingResourceAssets()
    {
        string subnauticaPath = NitroxUser.GamePath;
        if (string.IsNullOrEmpty(subnauticaPath))
        {
            throw new DirectoryNotFoundException("Could not locate Subnautica installation directory for resource parsing.");
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
