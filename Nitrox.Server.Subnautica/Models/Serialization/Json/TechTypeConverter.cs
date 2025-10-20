﻿using System;
using Newtonsoft.Json;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json
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
