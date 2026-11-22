using System.Text.Json;
using System.Text.Json.Serialization;
using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

internal sealed class SessionIdJsonConverter : JsonConverter<SessionId>
{
    public override SessionId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (SessionId)reader.GetUInt16();

    public override void Write(Utf8JsonWriter writer, SessionId value, JsonSerializerOptions options) => writer.WriteNumberValue((ushort)value);
}
