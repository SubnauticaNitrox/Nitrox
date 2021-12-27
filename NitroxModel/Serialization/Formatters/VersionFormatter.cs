using System;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace NitroxModel.Serialization.Formatters;

internal class VersionFormatter<TTypeResolver> : Formatter<TTypeResolver, Version>
    where TTypeResolver : ITypeResolver, new()
{
    public override int? GetLength() => null;

    // Converts the version to a string and serializes the string
    public override int Serialize(ref byte[] bytes, int offset, Version value)
    {
        return Formatter<TTypeResolver, string>.Default.Serialize(ref bytes, offset, value.ToString());
    }

    public override Version Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
    {
        string versionString = Formatter<TTypeResolver, string>.Default.Deserialize(ref bytes, offset, tracker, out byteSize);
        return (versionString == null) ? null : new Version(versionString);
    }
}
