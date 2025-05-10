extern alias JB;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Database.Converters;

// TODO: Optimize (use different Sqlite data type?)
internal sealed class NitroxQuaternionConverter() : ValueConverter<NitroxQuaternion, byte[]>(static id => ToBytes(id),
                                                                                             static bytes => ToValue(bytes))
{
    static byte[] ToBytes(NitroxQuaternion input)
    {
        byte[] result = new byte[16];
        Array.Copy(BitConverter.GetBytes(input.X), result, 4);
        Array.Copy(BitConverter.GetBytes(input.Y), 0, result, 4, 4);
        Array.Copy(BitConverter.GetBytes(input.Z), 0, result, 8, 4);
        Array.Copy(BitConverter.GetBytes(input.W), 0, result, 12, 4);
        return result;
    }

    static NitroxQuaternion ToValue(byte[] bytes)
    {
        NitroxQuaternion result = default;
        result.X = BitConverter.ToSingle(bytes, 0);
        result.Y = BitConverter.ToSingle(bytes, 4);
        result.Z = BitConverter.ToSingle(bytes, 8);
        result.W = BitConverter.ToSingle(bytes, 12);
        return result;
    }
}
