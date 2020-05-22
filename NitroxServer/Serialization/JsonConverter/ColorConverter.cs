using System;
using Nitrox.Newtonsoft.Json;
using Nitrox.Newtonsoft.Json.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class ColorConverter : JsonConverter<NitroxColor>
    {
        public override void WriteJson(JsonWriter writer, NitroxColor value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("R");
            writer.WriteValue(value.R);
            writer.WritePropertyName("G");
            writer.WriteValue(value.G);
            writer.WritePropertyName("B");
            writer.WriteValue(value.B);
            writer.WritePropertyName("A");
            writer.WriteValue(value.A);
            writer.WriteEndObject();
        }

        public override NitroxColor ReadJson(JsonReader reader, Type objectType, NitroxColor existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return JObject.Load(reader).ToObject<NitroxColor>();
        }
    }
}
