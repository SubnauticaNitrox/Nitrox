using System;
using Nitrox.Newtonsoft.Json;
using TechTypeModel = NitroxModel.DataStructures.TechType;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class TechTypeConverter : JsonConverter<TechTypeModel>
    {
        public override void WriteJson(JsonWriter writer, TechTypeModel value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Name);
        }
        public override TechTypeModel ReadJson(JsonReader reader, Type objectType, TechTypeModel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? null : new TechTypeModel((string)reader.Value);
        }
    }
}
