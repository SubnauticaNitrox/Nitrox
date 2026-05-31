using Newtonsoft.Json;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

internal sealed class PeerIdConverter : JsonConverter<PeerId>
{
    public override void WriteJson(JsonWriter writer, PeerId value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }

    public override PeerId ReadJson(JsonReader reader, Type objectType, PeerId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is { } num)
        {
            return Convert.ToUInt32(reader.Value);
        }
        if (hasExistingValue)
        {
            return existingValue;
        }
        return 0;
    }
}
