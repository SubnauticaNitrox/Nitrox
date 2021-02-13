using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using NitroxModel.Discovery;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers;
using NitroxServer_Subnautica.Serialization.Resources.Parsers.Images;
using NitroxServer_Subnautica.Serialization.Resources.Processing;

namespace NitroxServer_Subnautica.Serialization.Resources
{
    public static class ResourceAssetsParser
    {
        private static ResourceAssets resourceAssets;

        private static readonly Dictionary<AssetIdentifier, uint> assetIdentifierToClassId = new Dictionary<AssetIdentifier, uint>();

        private static readonly Dictionary<string, int> fileIdByResourcePath = new Dictionary<string, int>();
        private static readonly HashSet<string> parsedManifests = new HashSet<string>();

        private static readonly PrefabPlaceholderExtractor prefabPlaceholderExtractor = new PrefabPlaceholderExtractor();

        // https://docs.huihoo.com/unity/4.3/Documentation/Manual/ClassIDReference.html
        private static readonly Dictionary<uint, AssetParser> assetParsersByClassId = new Dictionary<uint, AssetParser>()
        {
            { 1, new GameObjectAssetParser()},
            { 4, new TransformAssetParser()},
            { 28, new Texture2DAssetParser() },
            { 49, new TextAssetParser() },
            { 114, new MonobehaviourAssetParser() },
            { 115, new MonoscriptAssetParser() },

            // 224 is RectTransform. We don't currently directly interpret it; however, we want to parse it so that
            // any transform links are maintained.  All of the beginning fields are exactly the same as a regular
            // transform object.  It just has some additional metadata that is currently unused.
            { 224, new TransformAssetParser()}
        };

        public static ResourceAssets Parse()
        {
            if (resourceAssets != null)
            {
                return resourceAssets;
            }

            TryParseAllAssetsFiles(FindDirectoryContainingResourceAssets(), out resourceAssets);

            prefabPlaceholderExtractor.LoadInto(resourceAssets);

            ResourceAssets.ValidateMembers(resourceAssets);

            return resourceAssets;
        }


        public static bool TryParseAllAssetsFiles(string basePath, out ResourceAssets resourceAssets)
        {
            resourceAssets = new ResourceAssets();
            foreach (string fileName in Directory.GetFiles(basePath, "*.assets"))
            {
                ParseAssetManifest(basePath, fileName, resourceAssets);
            }

            return true;
        }


        private static void ParseAssetManifest(string basePath, string fileName, ResourceAssets resourceAssets)
        {
            string path = Path.Combine(basePath, fileName).Replace("resources/", "Resources/");

            if (parsedManifests.Contains(path))
            {
                return;
            }
            Dictionary<int, string> relativeFileIdToPath = new Dictionary<int, string>();

            using (FileStream resStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (AssetsFileReader reader = new AssetsFileReader(resStream))
            {
                AssetsFile file = new AssetsFile(reader);
                AssetsFileTable resourcesFileTable = new AssetsFileTable(file);

                parsedManifests.Add(path);


                relativeFileIdToPath.Add(0, path);
                int fileId = 1;
                foreach (AssetsFileDependency dependency in file.dependencies.dependencies)
                {
                    relativeFileIdToPath.Add(fileId++, Path.Combine(basePath, dependency.assetPath));
                }

                foreach (AssetsFileDependency dependency in file.dependencies.dependencies)
                {
                    ParseAssetManifest(basePath, dependency.assetPath, resourceAssets);
                }

                foreach (AssetFileInfoEx assetFileInfo in resourcesFileTable.assetFileInfo)
                {
                    reader.Position = assetFileInfo.absoluteFilePos;

                    AssetIdentifier identifier = new AssetIdentifier(path, assetFileInfo.index);


                    if (assetParsersByClassId.TryGetValue(assetFileInfo.curFileType, out AssetParser assetParser))
                    {
                        assetParser.Parse(identifier, reader, resourceAssets, relativeFileIdToPath);
                    }

                    assetIdentifierToClassId.Add(identifier, assetFileInfo.curFileType);
                }
            }
        }

        private static string FindDirectoryContainingResourceAssets()
        {
            List<string> errors = new List<string>();
            string subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);
            if (subnauticaPath == null)
            {
                throw new DirectoryNotFoundException($"Could not locate Subnautica installation directory:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
#if DEBUG
            if (File.Exists(Path.Combine(subnauticaPath, "Subnautica_Data", "resources.assets")))
            {
                return Path.Combine(subnauticaPath, "Subnautica_Data");
            }
            if (File.Exists(Path.Combine("..", "resources.assets")))   //  SubServer => Subnautica/Subnautica_Data/SubServer
            {
                return Path.GetFullPath(Path.Combine(".."));
            }
            if (File.Exists(Path.Combine("..", "Subnautica_Data", "resources.assets")))   //  SubServer => Subnautica/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", "Subnautica_Data"));
            }
#elif BELOWZERO
            if (File.Exists(Path.Combine(subnauticaPath, "SubnauticaZero_Data", "resources.assets")))
            {
                return Path.Combine(subnauticaPath, "SubnauticaZero_Data");
            }
            if (File.Exists(Path.Combine("..", "resources.assets")))   //  SubServer => SubnauticaZero/SubnauticaZero_Data/SubServer
            {
                return Path.GetFullPath(Path.Combine(".."));
            }
            if (File.Exists(Path.Combine("..", "SubnauticaZero_Data", "resources.assets")))   //  SubServer => SubnauticaZero/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", "SubnauticaZero_Data"));
            }
#endif
            if (File.Exists("resources.assets"))   //  SubServer/* => SubnauticaZero/SubnauticaZero_Data/
            {
                return Directory.GetCurrentDirectory();
            }
            throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
        }
    }
}
