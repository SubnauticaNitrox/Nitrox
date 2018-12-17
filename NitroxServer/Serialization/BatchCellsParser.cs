using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;

namespace NitroxServer.Serialization
{
    /**
     * Parses the files in build18 in the format of batch-cells-x-y-z-slot-type.bin
     * These files contain serialized GameObjects with EntitySlot components. These
     * represent areas that entities (creatures, objects) can spawn within the world.
     * This class consolidates the gameObject, entitySlot, and cellHeader data to
     * create EntitySpawnPoint objects.
     */
    class BatchCellsParser
    {
        public bool WarnSilence;
        public bool DoNotAttempt;
        private readonly ServerProtobufSerializer serializer;
        private readonly Dictionary<string, Type> surrogateTypes = new Dictionary<string, Type>();

        public void CheckForIndexAndSilenceWarnings()
        {
            /**
           * Implemented check to see if index.txt in our Build18 folder exists.
           * If it does, warnings for this check are then silenced on the console, and instead, missing files are dumped to a missing.txt file
           * Warnings can be turned back on by the presence of a nosilence.txt file. Could be later implemented as an option. */
            if (WarnSilence)
            {
                return;
            }
            if (DoNotAttempt)
            {
                return;
            }
            string PathToSub;
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);
            if (subnauticaPath.IsEmpty())
            {
                string pathTXTFile = Path.Combine(Path.GetFullPath("."), "path.txt");
                Log.Info("Using " + pathTXTFile + " as path to path.txt");
                PathToSub = File.ReadAllText(pathTXTFile);
            } else
            {
                PathToSub = subnauticaPath.Get();
            }
            if (PathToSub.Length <= 1)
            {
                Log.Warn("Empty Path.txt file or universal finder can't find game directory! Can not continue executing method!");
                return;
            }
            string fileName = Path.Combine(PathToSub, "SNUnmanagedData\\Build18\\index.txt");
            Log.Info("Checking for " + fileName);
            if (!File.Exists(fileName))
            {
                Log.Warn("Can not find index.txt!");
                return;
            }
            fileName = Path.Combine(".\\", "nosilence.txt");
            if (!File.Exists(fileName))
            {
                Log.Info("Index found! Warnings about missing files are silenced and missing filenames are now dumped to missing.txt!");
                WarnSilence = true;
            }
            else
            {
                Log.Info("Not silencing warnings about missing files!");
                DoNotAttempt = true;
            }
            return;
        }
        public void LogMissingFileName(string fileName)
        {
            string missingTXTFile = Path.Combine(Path.GetFullPath("."), "missing.txt");
            if (!File.Exists(missingTXTFile))
            {
                StreamWriter missingtxt = File.CreateText(missingTXTFile);
                missingtxt.WriteLine(fileName);
                missingtxt.Close();
            }
            else
            {
                StreamWriter missingtxt = File.AppendText(missingTXTFile);
                missingtxt.WriteLine(fileName);
                missingtxt.Close();
            }
        }

        public BatchCellsParser()
        {
            serializer = new ServerProtobufSerializer();

            surrogateTypes.Add("UnityEngine.Transform", typeof(Transform));
            surrogateTypes.Add("UnityEngine.Vector3", typeof(Vector3));
            surrogateTypes.Add("UnityEngine.Quaternion", typeof(Quaternion));
        }

        public List<EntitySpawnPoint> ParseBatchData(Int3 batchId)
        {
            List<EntitySpawnPoint> spawnPoints = new List<EntitySpawnPoint>();

            ParseFile(batchId, "", "loot-slots", spawnPoints);
            ParseFile(batchId, "", "creature-slots", spawnPoints);
            ParseFile(batchId, @"Generated\", "slots", spawnPoints);  // Very expensive to load
            ParseFile(batchId, "", "loot", spawnPoints);
            ParseFile(batchId, "", "creatures", spawnPoints);
            ParseFile(batchId, "", "other", spawnPoints);

            Log.Debug($"Loaded {spawnPoints.Count} entity-spawn-points for batch {batchId}");

            return spawnPoints;
        }

        public void ParseFile(Int3 batchId, string pathPrefix, string suffix, List<EntitySpawnPoint> spawnPoints)
        {
            CheckForIndexAndSilenceWarnings();
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);

            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                return;
            }

            string path = Path.Combine(subnauticaPath.Get(), "SNUnmanagedData/Build18");
            string fileName = Path.Combine(path, pathPrefix + "batch-cells-" + batchId.x + "-" + batchId.y + "-" + batchId.z + "-" + suffix + ".bin");

            if (!File.Exists(fileName))
            {
                if (WarnSilence)
                {
                    LogMissingFileName(fileName);
                    return;
                }
                else
                {
                    Log.Info($"Unable to find batch cells file '{fileName}'! Please move SNUnmanagedData\\Build18 to {Path.Combine(Directory.GetCurrentDirectory(), @"SNUnmanagedData\Build18")}.");
                    return;
                }
            }

            using (Stream stream = File.OpenRead(fileName))
            {
                CellManager.CellsFileHeader cellsFileHeader = serializer.Deserialize<CellManager.CellsFileHeader>(stream);

                for (int cellCounter = 0; cellCounter < cellsFileHeader.numCells; cellCounter++)
                {
                    CellManager.CellHeader cellHeader = serializer.Deserialize<CellManager.CellHeader>(stream);
                    ProtobufSerializer.LoopHeader gameObjectCount = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

                    for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
                    {
                        GameObject gameObject = DeserializeGameObject(stream);

                        EntitySpawnPoint esp = EntitySpawnPoint.From(batchId, gameObject, cellHeader);
                        spawnPoints.Add(esp);
                    }
                }
            }
        }

        private GameObject DeserializeGameObject(Stream stream)
        {
            ProtobufSerializer.GameObjectData goData = serializer.Deserialize<ProtobufSerializer.GameObjectData>(stream);

            GameObject gameObject = new GameObject(goData);
            DeserializeComponents(stream, gameObject);

            return gameObject;
        }

        private void DeserializeComponents(Stream stream, GameObject gameObject)
        {
            ProtobufSerializer.LoopHeader components = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

            for (int componentCounter = 0; componentCounter < components.Count; componentCounter++)
            {
                ProtobufSerializer.ComponentHeader componentHeader = serializer.Deserialize<ProtobufSerializer.ComponentHeader>(stream);

                Type type = null;

                if (!surrogateTypes.TryGetValue(componentHeader.TypeName, out type))
                {
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.GetType(componentHeader.TypeName))
                        .FirstOrDefault(t => t != null);
                }

                Validate.NotNull(type, $"No type or surrogate found for {componentHeader.TypeName}!");

                object component = FormatterServices.GetUninitializedObject(type);
                serializer.Deserialize(stream, component, type);

                gameObject.AddComponent(component, type);
            }
        }
    }
}
