using System.IO;
using Newtonsoft.Json;
using NitroxModel.DataStructures.JsonConverter;
using NitroxModel.Logger;

namespace NitroxServer.Serialization
{
    public class ServerJsonSerializer : IServerSerializer
    {
        private readonly JsonSerializer serializer;

        public ServerJsonSerializer()
        {
            serializer = new JsonSerializer();

            serializer.Error += delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
            {
                Log.Error(e.ErrorContext.Error, "Json serialization error: ");
            };

            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.ContractResolver = new AttributeContractResolver();
            serializer.Converters.Add(new NitroxIdConverter());
            serializer.Converters.Add(new TechTypeConverter());
        }

        public void Serialize(Stream stream, object o)
        {
            stream.Position = 0;
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(stream)))
            {
                serializer.Serialize(jsonTextWriter, o);
            }
        }

        public void Serialize(string filePath, object o)
        {
            using (StreamWriter stream = File.CreateText(filePath))
            {
                serializer.Serialize(stream, o);
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            stream.Position = 0;
            using (JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(stream)))
            {
                return (T)serializer.Deserialize(jsonTextReader, typeof(T));
            }
        }

        public T Deserialize<T>(string filePath)
        {
            using (StreamReader jsonTextReader = File.OpenText(filePath))
            {
                return (T)serializer.Deserialize(jsonTextReader, typeof(T));
            }
        }
    }
}
