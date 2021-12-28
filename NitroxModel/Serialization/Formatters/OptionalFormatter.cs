using NitroxModel.DataStructures.Util;
using ZeroFormatter;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;

namespace NitroxModel.Serialization.Formatters;

internal class OptionalFormatter<TTypeResolver, T> : Formatter<TTypeResolver, Optional<T>>
    where TTypeResolver : ITypeResolver, new()
    where T : class
{
    public override int? GetLength() => null;

    public override int Serialize(ref byte[] bytes, int offset, Optional<T> value)
    {
        int startOffset = offset;

        offset += BinaryUtil.WriteBoolean(ref bytes, offset, value.HasValue);

        if (value.HasValue)
        {
            offset += Formatter<TTypeResolver, T>.Default.Serialize(ref bytes, offset, value.Value);
        }

        return offset - startOffset;
    }

    public override Optional<T> Deserialize(ref byte[] bytes, int offset, DirtyTracker tracker, out int byteSize)
    {
        byteSize = 1;
        bool hasValue = BinaryUtil.ReadBoolean(ref bytes, offset);

        if (!hasValue)
        {
            return Optional.Empty;
        }

        offset += 4;

        T value = Formatter<TTypeResolver, T>.Default.Deserialize(ref bytes, offset, tracker, out int size);
        offset += size;

        return Optional.Of(value);
    }
}
