using System;
using Newtonsoft.Json;
using NitroxModel.DataStructures;

namespace NitroxServer.Serialization.JSON
{
    public class NitroxIdConverter : JsonConverter<NitroxId>
    {
        public override void WriteJson(JsonWriter writer, NitroxId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override NitroxId ReadJson(JsonReader reader, Type objectType, NitroxId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? null : new NitroxId((string)reader.Value);
        }
    }
}
