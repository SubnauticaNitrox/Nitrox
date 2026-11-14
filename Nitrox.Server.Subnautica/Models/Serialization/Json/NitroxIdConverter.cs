using System.Text.Json;
using System.Text.Json.Serialization;
using Nitrox.Model.DataStructures;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

internal sealed class NitroxIdConverter : JsonConverter<NitroxId>
{
    public override NitroxId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();
        return value == null ? null : new NitroxId(value);
    }

    public override void Write(Utf8JsonWriter writer, NitroxId? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}
