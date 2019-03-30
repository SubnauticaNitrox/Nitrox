using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Logger;
using NitroxServer_Subnautica.Serialization.Resources.Parsers;
using NitroxServer_Subnautica.Serialization.Resources.Processing;

namespace NitroxServer_Subnautica.Serialization.Resources
{
    public static class ResourceAssetsParser
    {
        private static Dictionary<AssetIdentifier, uint> assetIdentifierToClassId = new Dictionary<AssetIdentifier, uint>();
        
        private static Dictionary<string, uint> fileIdByResourcePath = new Dictionary<string, uint>();
        private static HashSet<string> parsedManifests = new HashSet<string>();

        private static PrefabPlaceholderExtractor prefabPlaceholderExtractor = new PrefabPlaceholderExtractor();
        
        // https://docs.huihoo.com/unity/4.3/Documentation/Manual/ClassIDReference.html
        private static Dictionary<uint, AssetParser> assetParsersByClassId = new Dictionary<uint, AssetParser>()
        {
            { 1, new GameObjectAssetParser()},
            { 4, new TransformAssetParser()},
            { 49, new TextAssetParser() },
            { 114, new MonobehaviourAssetParser() },
            { 115, new MonoscriptAssetParser() }
        };

        public static ResourceAssets Parse()
        {
            ResourceAssets resourceAssets = new ResourceAssets();

            string basePath = FindDirectoryContainingResourceAssets();

            CalculateDependencyFileIds(basePath, "resources.assets");

            uint rootAssetId = 0; // resources.assets is always considered to be the top level '0'
            ParseAssetManifest(basePath, "resources.assets", rootAssetId, resourceAssets);

            prefabPlaceholderExtractor.LoadInto(resourceAssets);

            resourceAssets.ValidateMembers();

            return resourceAssets;
        }

        private static void ParseAssetManifest(string basePath, string fileName, uint fileId, ResourceAssets resourceAssets)
        {
            if(parsedManifests.Contains(fileName))
            {
                return;
            }

            parsedManifests.Add(fileName);

            string path = Path.Combine(basePath, fileName);

            using (FileStream resStream = new FileStream(path, FileMode.Open))
            using (AssetsFileReader reader = new AssetsFileReader(resStream))
            {
                AssetsFile file = new AssetsFile(reader);
                AssetsFileTable resourcesFileTable = new AssetsFileTable(file);
                
                foreach (AssetsFileDependency dependency in file.dependencies.pDependencies) 
                {
                    uint dependencyFileId = fileIdByResourcePath[dependency.assetPath];
                    ParseAssetManifest(basePath, dependency.assetPath, dependencyFileId, resourceAssets);
                }
                
                foreach (AssetFileInfoEx assetFileInfo in resourcesFileTable.pAssetFileInfo)
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

            using (FileStream resStream = new FileStream(path, FileMode.Open))
            using (AssetsFileReader reader = new AssetsFileReader(resStream))
            {
                AssetsFile file = new AssetsFile(reader);
                AssetsFileTable resourcesFileTable = new AssetsFileTable(file);
                
                uint fileId = 1;

                foreach (AssetsFileDependency dependency in file.dependencies.pDependencies)
                {
                    fileIdByResourcePath.Add(dependency.assetPath, fileId);
                    fileId++;
                }                
            }
        }

        private static string FindDirectoryContainingResourceAssets()
        {
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);

            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
            
            if (File.Exists(Path.Combine(subnauticaPath.Get(), "Subnautica_Data", "resources.assets")))
            {
                return Path.Combine(subnauticaPath.Get(), "Subnautica_Data");
            }
            else if (File.Exists(Path.Combine("..", "resources.assets")))   //  SubServer => Subnautica/Subnautica_Data/SubServer
            {
                return Path.GetFullPath(Path.Combine(".."));
            }
            else if (File.Exists(Path.Combine("..", "Subnautica_Data", "resources.assets")))   //  SubServer => Subnautica/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", "Subnautica_Data"));
            }
            else if (File.Exists("resources.assets"))   //  SubServer/* => Subnautica/Subnautica_Data/
            {
                return Directory.GetCurrentDirectory();
            }
            else
            {
                throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
            }
        }
    }
}
