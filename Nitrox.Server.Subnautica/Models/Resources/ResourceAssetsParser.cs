using System;
using System.IO;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using Nitrox.Server.Subnautica.Models.Resources.Parsers.Abstract;

namespace Nitrox.Server.Subnautica.Models.Resources;

public static class ResourceAssetsParser
{
    private static ResourceAssets? resourceAssets;

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
        if (string.IsNullOrEmpty(NitroxUser.GamePath))
        {
            throw new Exception($"{nameof(NitroxUser.GamePath)} was null or empty. Be sure {nameof(GameInstallationFinder.FindGameCached)} was executed.");
        }

        if (File.Exists(Path.Combine(NitroxUser.GamePath, GameInfo.Subnautica.DataFolder, "resources.assets")))
        {
            return Path.Combine(NitroxUser.GamePath, GameInfo.Subnautica.DataFolder);
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
    }
}
