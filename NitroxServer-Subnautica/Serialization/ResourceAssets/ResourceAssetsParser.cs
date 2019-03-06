using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;
using NitroxServer.GameLogic;
using NitroxServer.UnityStubs;
using UWE;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;
using NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers.Abstract;
using NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets
{
    public static class ResourceAssetsParser
    {
        private static Dictionary<string, List<UwePrefab>> gameObjectsByClassId = new Dictionary<string, List<UwePrefab>>();
        private static Dictionary<string, MonoBehaviourParser> parsers = new Dictionary<string, MonoBehaviourParser>()
        {
            { "ReefbackSlotsData", new ReefbackSlotsDataParser() },
            { "WorldEntityData", new WorldEntityDataParser() }
        };

        private static DefaultMonoBehaviourParser defaultMonoBehaviourParser = new DefaultMonoBehaviourParser();

        private static AssetsFileInstance instance;
        private static AssetsManager manager;

        private static HashSet<long> checkedGameObjects = new HashSet<long>();
        private static HashSet<ulong> matchingMonoscripts = new HashSet<ulong>();

        private const uint TEXT_CLASS_ID = 0x31;
        private const uint MONOBEHAVIOUR_CLASS_ID = 0x72;

        public static ResourceAssets Parse()
        {
            ResourceAssets resourceAssets = new ResourceAssets();

            SetupAssetManager();

            AssetsFileReader resReader = new AssetsFileReader(instance.stream);
            AssetsFile resourcesFile = instance.file;
            AssetsFileReader afr = resourcesFile.reader;
            AssetsFileTable resourcesFileTable = instance.table;

            int timesRan = 0;
            foreach (AssetFileInfoEx afi in resourcesFileTable.pAssetFileInfo.Values)
            {
                if (afi.curFileType == TEXT_CLASS_ID)
                {
                    resourcesFile.reader.Position = afi.absoluteFilePos;
                    string assetName = resourcesFile.reader.ReadCountStringInt32();
                    if (assetName == "EntityDistributions")
                    {
                        resourcesFile.reader.Align();
                        resourceAssets.LootDistributionsJson = resourcesFile.reader.ReadCountStringInt32().Replace("\\n", "");
                    }
                }
                else if (afi.curFileType == MONOBEHAVIOUR_CLASS_ID)
                {
                    resourcesFile.reader.Position = afi.absoluteFilePos;
                    int gameObjectFileId = resourcesFile.reader.ReadInt32();
                    long gameObjectPathId = resourcesFile.reader.ReadInt64();

                    resourcesFile.reader.Position += 16;
                    string assetName = resourcesFile.reader.ReadCountStringInt32();
                    MonoBehaviourParser val;
                    if (!parsers.TryGetValue(assetName, out val))
                    {
                        defaultMonoBehaviourParser.Parse(instance, gameObjectPathId, resourceAssets);
                    }
                    else
                    {
                        val.Parse(instance, gameObjectPathId, resourceAssets);
                    }
                }
            }
            Validate.IsTrue(resourceAssets.WorldEntitiesByClassId.Count > 0);
            Validate.IsTrue(resourceAssets.LootDistributionsJson != "");

            return resourceAssets;
        }

        private static void SetupAssetManager()
        {
            manager = new AssetsManager();
            instance = manager.LoadAssetsFile(new FileStream(FindPath(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), true);
        }

        private static string FindAssemblyPath(Assembly assembly)
        {
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);

            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors.ToArray())}");
                return assembly.FullName;
            }

            return Path.Combine(subnauticaPath.Get(), Path.Combine("SubnauticaData", Path.Combine("Managed", assembly.FullName)));
        }

        private static string FindPath()
        {
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);
            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors.ToArray())}");
            }
            
            string gameResourcesPath = "";

            if (!subnauticaPath.IsEmpty())
            {
                gameResourcesPath = Path.Combine(subnauticaPath.Get(), Path.Combine("Subnautica_Data", "resources.assets"));
            }

            if (File.Exists(gameResourcesPath))
            {
                return gameResourcesPath;
            }
            else if (File.Exists(Path.Combine("..", "resources.assets")))   //  SubServer => Subnautica/Subnautica_Data/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", "resources.assets"));
            }
            else if (File.Exists(Path.Combine("..", Path.Combine("Subnautica_Data", "resources.assets"))))   //  SubServer => Subnautica/SubServer
            {
                return Path.GetFullPath(Path.Combine("..", Path.Combine("Subnautica_Data", "resources.assets")));
            }
            else if (File.Exists("resources.assets"))   //  SubServer/* => Subnautica/Subnautica_Data/
            {
                return Path.GetFullPath("resources.assets");
            }
            else
            {
                throw new FileNotFoundException("Make sure resources.assets is in current or parent directory and readable.");
            }
        }
    }
}
