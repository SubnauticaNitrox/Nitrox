using System.IO;
using NitroxModel;
using NitroxModel.Helper;
using NitroxServer_Subnautica.Resources.Parsers;

namespace NitroxServer_Subnautica.Resources;

public static class ResourceAssetsParser
{
    private static ResourceAssets resourceAssets;

    public static ResourceAssets Parse()
    {
        if (resourceAssets != null)
        {
            return resourceAssets;
        }

        using (PrefabPlaceholderGroupsParser prefabPlaceholderGroupsParser = new())
        {
            resourceAssets = new ResourceAssets
            {
                WorldEntitiesByClassId = new WorldEntityInfoParser().ParseFile(),
                LootDistributionsJson = new EntityDistributionsParser().ParseFile(),
                PrefabPlaceholdersGroupsByGroupClassId = prefabPlaceholderGroupsParser.ParseFile(),
                NitroxRandom = new RandomStartParser().ParseFile(),
                RandomPossibilitiesByClassId = new(prefabPlaceholderGroupsParser.RandomPossibilitiesByClassId)
            };
        }
        AssetParser.Dispose();

        ResourceAssets.ValidateMembers(resourceAssets);
        return resourceAssets;
    }

    public static string FindDirectoryContainingResourceAssets()
    {
        string subnauticaPath = NitroxUser.GamePath;
        //subnauticaPath = "E:\\Program Files\\Steam\\steamapps\\common\\Subnautica";
#if SUBNAUTICA
        if (string.IsNullOrEmpty(subnauticaPath))
        {
            throw new DirectoryNotFoundException("Could not locate Subnautica installation directory for resource parsing.");
        }

        if (File.Exists(Path.Combine(subnauticaPath, GameInfo.Subnautica.DataFolder, "resources.assets")))
        {
            return Path.Combine(subnauticaPath, GameInfo.Subnautica.DataFolder);
        }
        if (File.Exists(Path.Combine("..", "resources.assets"))) //  SubServer => Subnautica/Subnautica_Data/SubServer
        {
            return Path.GetFullPath(Path.Combine(".."));
        }
        if (File.Exists(Path.Combine("..", GameInfo.Subnautica.DataFolder, "resources.assets"))) //  SubServer => Subnautica/SubServer
        {
            return Path.GetFullPath(Path.Combine("..", GameInfo.Subnautica.DataFolder));
        }
        if (File.Exists("resources.assets")) //  SubServer/* => Subnautica/Subnautica_Data/
        {
            return Directory.GetCurrentDirectory();
        }
        throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
#elif BELOWZERO
        if (string.IsNullOrEmpty(subnauticaPath))
        {
            throw new DirectoryNotFoundException("Could not locate Subnautica Below Zero installation directory for resource parsing.");
        }

        if (File.Exists(Path.Combine(subnauticaPath, "SubnauticaZero_Data", "resources.assets")))
        {
            return Path.Combine(subnauticaPath, "SubnauticaZero_Data");
        }
        if (File.Exists(Path.Combine("..", "resources.assets"))) //  SubServer => SubnauticaZero/SubnauticaZero_Data/SubServer
        {
            return Path.GetFullPath(Path.Combine(".."));
        }
        if (File.Exists(Path.Combine("..", "SubnauticaZero_Data", "resources.assets"))) //  SubServer => SubnauticaZero/SubServer
        {
            return Path.GetFullPath(Path.Combine("..", "SubnauticaZero_Data"));
        }
        if (File.Exists("resources.assets")) //  SubServer/* => SubnauticaZero/SubnauticaZero_Data/
        {
            return Directory.GetCurrentDirectory();
        }
        throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
#endif
    }
}
