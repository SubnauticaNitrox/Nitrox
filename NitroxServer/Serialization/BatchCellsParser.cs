using System.IO;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using NitroxServer.UnityStubs;

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

        public List<GameObject> ParseCreatureSlots(Int3 batchId)
        {
            String fileName = "batch-cells-" + batchId .x + "-" + batchId.y + "-" + batchId.z + "-creature-slots.bin";
            List<GameObject> gameObjects = new List<GameObject>();

            using (Stream stream = FileUtils.ReadFile(fileName))
            {
                CellManager.CellsFileHeader cellsFileHeader = serializer.Deserialize<CellManager.CellsFileHeader>(stream);

                for (int cellCounter = 0; cellCounter < cellsFileHeader.numCells; cellCounter++)
                {
                    CellManager.CellHeader cellHeader = serializer.Deserialize<CellManager.CellHeader>(stream);
                    ProtobufSerializer.LoopHeader gameObjectCount = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

                    for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
                    {
                        ProtobufSerializer.GameObjectData goData = serializer.Deserialize<ProtobufSerializer.GameObjectData>(stream);
                        ProtobufSerializer.LoopHeader componentCount = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

                        GameObject gameObject = new GameObject(goData);
                        gameObjects.Add(gameObject);

                        for (int componentCounter = 0; componentCounter < componentCount.Count; componentCounter++)
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

                                    if(type != null)
                                    {
                                        break;
                                    }
                                }
                            }

                            var component = FormatterServices.GetUninitializedObject(type);
                            serializer.Deserialize(stream, component, type);

                            gameObject.AddComponent(component, type);
                        }
                    }
                }
            }

            return gameObjects;
        } 
    }    
}
