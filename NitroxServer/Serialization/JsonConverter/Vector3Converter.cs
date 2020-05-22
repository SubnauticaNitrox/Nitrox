using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class Vector3Converter : JsonConverter<NitroxVector3>
    {
        public override void WriteJson(JsonWriter writer, NitroxVector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(value.Z);
            writer.WriteEndObject();
        }

        public override NitroxVector3 ReadJson(JsonReader reader, Type objectType, NitroxVector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return JObject.Load(reader).ToObject<NitroxVector3>();
        }
    }
}
