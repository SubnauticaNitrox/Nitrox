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
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                streamWriter.AutoFlush = true;
                serializer.Serialize(streamWriter, o);
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            return (T)serializer.Deserialize(new StreamReader(stream), typeof(T));
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
