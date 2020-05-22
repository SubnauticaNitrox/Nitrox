using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class QuaternionConverter : JsonConverter<NitroxQuaternion>
    {
        public override void WriteJson(JsonWriter writer, NitroxQuaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(value.Z);
            writer.WritePropertyName("W");
            writer.WriteValue(value.W);
            writer.WriteEndObject();
        }

        public override NitroxQuaternion ReadJson(JsonReader reader, Type objectType, NitroxQuaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return JObject.Load(reader).ToObject<NitroxQuaternion>();
        }
    }
}
