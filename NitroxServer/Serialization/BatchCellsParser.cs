using System.IO;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using NitroxServer.UnityStubs;
using NitroxModel.Logger;

namespace NitroxServer.Serialization
{
    class BatchCellsParser
    {
        private ServerProtobufSerializer serializer;
        private Dictionary<String, Type> surrogateTypes = new Dictionary<string, Type>();

        public BatchCellsParser()
        {
            serializer = new ServerProtobufSerializer();

            surrogateTypes.Add("UnityEngine.Transform", typeof(Transform));
            surrogateTypes.Add("UnityEngine.Vector3", typeof(Vector3));
            surrogateTypes.Add("UnityEngine.Quaternion", typeof(Quaternion));
        }

        public List<GameObject> ParseGameObjects(Int3 batchId)
        {
            List<GameObject> allGameObjects = new List<GameObject>();
            Dictionary<String, GameObject> gameObjectsByClassId = new Dictionary<String, GameObject>();
            
            ParseFile(batchId, "", "loot-slots", allGameObjects, gameObjectsByClassId);
            ParseFile(batchId, "", "creature-slots", allGameObjects, gameObjectsByClassId);
            ParseFile(batchId, @"Generated\", "slots", allGameObjects, gameObjectsByClassId);
            ParseFile(batchId, "", "loot", allGameObjects, gameObjectsByClassId);
            ParseFile(batchId, "", "creatures", allGameObjects, gameObjectsByClassId);
            ParseFile(batchId, "", "other", allGameObjects, gameObjectsByClassId);

            return allGameObjects;
        }

        public void ParseFile(Int3 batchId, String pathPrefix, String suffix, List<GameObject> allGameObjects, Dictionary<String, GameObject> gameObjectsByClassId)
        {          
            String fileName = pathPrefix + "batch-cells-" + batchId .x + "-" + batchId.y + "-" + batchId.z + "-" + suffix + ".bin";

            using (Stream stream = FileUtils.ReadFile(fileName))
            {
                CellManager.CellsFileHeader cellsFileHeader = serializer.Deserialize<CellManager.CellsFileHeader>(stream);

                for (int cellCounter = 0; cellCounter < cellsFileHeader.numCells; cellCounter++)
                {
                    CellManager.CellHeader cellHeader = serializer.Deserialize<CellManager.CellHeader>(stream);
                    ProtobufSerializer.LoopHeader gameObjectCount = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

                    for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
                    {
                        DeserializeGameObject(stream, allGameObjects, gameObjectsByClassId);
                    }
                }
            }
        }

        private void DeserializeGameObject(Stream stream, List<GameObject> allGameObjects, Dictionary<String, GameObject> gameObjectsByClassId)
        {
            ProtobufSerializer.GameObjectData goData = serializer.Deserialize<ProtobufSerializer.GameObjectData>(stream);

            GameObject gameObject;

            if (goData.CreateEmptyObject || !gameObjectsByClassId.ContainsKey(goData.ClassId))
            {
                gameObject = new GameObject(goData);

                if (goData.ClassId != null)
                {
                    gameObjectsByClassId[goData.ClassId] = gameObject;
                }

                allGameObjects.Add(gameObject);
            }
            else
            {
                gameObject = gameObjectsByClassId[goData.ClassId];
            }

            DeserializeComponents(stream, gameObject);
        }

        private void DeserializeComponents(Stream stream, GameObject gameObject)
        {
            ProtobufSerializer.LoopHeader components = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

            for (int componentCounter = 0; componentCounter < components.Count; componentCounter++)
            {
                ProtobufSerializer.ComponentHeader componentHeader = serializer.Deserialize<ProtobufSerializer.ComponentHeader>(stream);

                Type type = null;

                if (surrogateTypes.ContainsKey(componentHeader.TypeName))
                {
                    type = surrogateTypes[componentHeader.TypeName];
                }
                else
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(componentHeader.TypeName);

                        if (type != null)
                        {
                            break;
                        }
                    }
                }

                var component = FormatterServices.GetUninitializedObject(type);
                serializer.Deserialize(stream, component, type);

                if (gameObject.GetComponent(type) == null)
                {
                    gameObject.AddComponent(component, type);
                }
                else
                {
                    Log.Info("GameObject(" + gameObject.Id + ") already has component of type " + type);
                }
            }
        }
    }    
}
