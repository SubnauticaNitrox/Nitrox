using System.Text.Json;
using System.Text.Json.Serialization;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

internal sealed class PeerIdJsonConverter : JsonConverter<PeerId>
{
    public override PeerId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (PeerId)reader.GetUInt32();

    public override void Write(Utf8JsonWriter writer, PeerId value, JsonSerializerOptions options) => writer.WriteNumberValue((uint)value);
}
