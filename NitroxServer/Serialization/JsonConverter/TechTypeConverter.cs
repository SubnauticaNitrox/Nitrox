using System;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class TechTypeConverter : JsonConverter<NitroxTechType>
    {
        public override void WriteJson(JsonWriter writer, NitroxTechType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Name);
        }
        public override NitroxTechType ReadJson(JsonReader reader, Type objectType, NitroxTechType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? null : new NitroxTechType((string)reader.Value);
        }
    }
}
