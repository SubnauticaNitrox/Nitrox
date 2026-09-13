using Newtonsoft.Json;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json.NewtonSoft;

internal sealed class PeerIdConverter : JsonConverter<PeerId>
{
    public override void WriteJson(JsonWriter writer, PeerId value, JsonSerializer serializer)
    {
        writer.WriteValue((uint)value);
    }

    public override PeerId ReadJson(JsonReader reader, Type objectType, PeerId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is { } num)
        {
            return (PeerId)Convert.ToUInt32(reader.Value);
        }
        if (hasExistingValue)
        {
            return existingValue;
        }
        return (PeerId)0;
    }
}
