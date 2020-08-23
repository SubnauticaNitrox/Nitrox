﻿using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using NitroxModel.Discovery;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer_Subnautica.Serialization.Resources.Parsers;
using NitroxServer_Subnautica.Serialization.Resources.Processing;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources
{
    public static class ResourceAssetsParser
    {
        private static readonly Dictionary<AssetIdentifier, uint> assetIdentifierToClassId = new Dictionary<AssetIdentifier, uint>();

        private static readonly Dictionary<string, int> fileIdByResourcePath = new Dictionary<string, int>();
        private static readonly HashSet<string> parsedManifests = new HashSet<string>();

        private static readonly PrefabPlaceholderExtractor prefabPlaceholderExtractor = new PrefabPlaceholderExtractor();

        // https://docs.huihoo.com/unity/4.3/Documentation/Manual/ClassIDReference.html
        private static readonly Dictionary<uint, AssetParser> assetParsersByClassId = new Dictionary<uint, AssetParser>()
        {
            { 1, new GameObjectAssetParser()},
            { 4, new TransformAssetParser()},
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
            ResourceAssets resourceAssets = new ResourceAssets();

            string basePath = FindDirectoryContainingResourceAssets();

            CalculateDependencyFileIds(basePath, "resources.assets");

            int rootAssetId = 0; // resources.assets is always considered to be the top level '0'
            ParseAssetManifest(basePath, "resources.assets", rootAssetId, resourceAssets);

            prefabPlaceholderExtractor.LoadInto(resourceAssets);

            resourceAssets.ValidateMembers();

            return resourceAssets;
        }

        private static void ParseAssetManifest(string basePath, string fileName, int fileId, ResourceAssets resourceAssets)
        {
            fileName = fileName.Replace("resources/", "Resources/");

            if (parsedManifests.Contains(fileName))
            {
                return;
            }

            parsedManifests.Add(fileName);

            string path = Path.Combine(basePath, fileName);

            using (FileStream resStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (AssetsFileReader reader = new AssetsFileReader(resStream))
            {
                AssetsFile file = new AssetsFile(reader);
                AssetsFileTable resourcesFileTable = new AssetsFileTable(file);

                foreach (AssetsFileDependency dependency in file.dependencies.dependencies)
                {
                    int dependencyFileId = fileIdByResourcePath[dependency.assetPath];
                    ParseAssetManifest(basePath, dependency.assetPath, dependencyFileId, resourceAssets);
                }

                foreach (AssetFileInfoEx assetFileInfo in resourcesFileTable.assetFileInfo)
                {
                    reader.Position = assetFileInfo.absoluteFilePos;

                    AssetIdentifier identifier = new AssetIdentifier(fileId, assetFileInfo.index);

                    AssetParser assetParser;

                    if (assetParsersByClassId.TryGetValue(assetFileInfo.curFileType, out assetParser))
                    {
                        assetParser.Parse(identifier, reader, resourceAssets);
                    }

                    assetIdentifierToClassId.Add(identifier, assetFileInfo.curFileType);
                }
            }
        }

        // All dependencies are stored in the root resource.assets file.  The order they
        // are listed corresponds to the fileId order.  We store this value, so we can
        // fetch the fileId of different assets to build AssetIdentifiers.
        private static void CalculateDependencyFileIds(string basePath, string fileName)
        {
            string path = Path.Combine(basePath, fileName);

            using (FileStream resStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (AssetsFileReader reader = new AssetsFileReader(resStream))
            {
                AssetsFile file = new AssetsFile(reader);
                AssetsFileTable resourcesFileTable = new AssetsFileTable(file);

                int fileId = 1;

                foreach (AssetsFileDependency dependency in file.dependencies.dependencies)
                {
                    fileIdByResourcePath.Add(dependency.assetPath, fileId);
                    fileId++;
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
            if (File.Exists("resources.assets"))   //  SubServer/* => Subnautica/Subnautica_Data/
            {
                return Directory.GetCurrentDirectory();
            }
            throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
        }
    }
}
