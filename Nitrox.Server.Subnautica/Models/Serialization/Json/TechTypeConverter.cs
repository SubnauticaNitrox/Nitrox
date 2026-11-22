using System.Text.Json;
using System.Text.Json.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

internal sealed class TechTypeConverter : JsonConverter<NitroxTechType>
{
    public override NitroxTechType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return new NitroxTechType(value);
    }

    public override void Write(Utf8JsonWriter writer, NitroxTechType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
