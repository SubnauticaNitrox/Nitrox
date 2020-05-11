using System.IO;
using Nitrox.Newtonsoft.Json;
using NitroxModel.DataStructures.JsonConverter;
using NitroxModel.Logger;

namespace NitroxServer.Serialization
{
    public class ServerJsonSerializer : IServerSerializer
    {
        private readonly JsonSerializer serializer;

        public ServerJsonSerializer()
        {
            serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new AttributeContractResolver()
            };

            serializer.Error += delegate (object sender, Nitrox.Newtonsoft.Json.Serialization.ErrorEventArgs e)
            {
                Log.Error("Error in JsonSerializer.", e.ErrorContext.Error);
            };

            RegisterConverters();
        }

        public void Serialize(Stream stream, object o)
        {
            stream.Position = 0;
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(stream)))
            {
                serializer.Serialize(jsonTextWriter, o);
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

        private void RegisterConverters()
        {
            serializer.Converters.Add(new ColorConverter());
            serializer.Converters.Add(new NitroxIdConverter());
            serializer.Converters.Add(new QuaternionConverter());
            serializer.Converters.Add(new TechTypeConverter());
            serializer.Converters.Add(new Vector3Converter());
        }
    }
}
