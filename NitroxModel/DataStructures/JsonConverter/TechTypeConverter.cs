using System;
using Nitrox.Newtonsoft.Json;

namespace NitroxModel.DataStructures.JsonConverter
{
    public class TechTypeConverter : JsonConverter<TechType>
    {
        public override void WriteJson(JsonWriter writer, TechType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Name);
        }

        public override TechType ReadJson(JsonReader reader, Type objectType, TechType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (hasExistingValue)
            {
                existingValue.Name = (string)reader.Value;
            }
            else
            {
                existingValue = new TechType((string)reader.Value);
            }
            return existingValue;
        }
    }
}
