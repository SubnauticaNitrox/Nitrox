extern alias JB;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Database.Converters;

// TODO: Optimize (use different Sqlite data type?)
internal sealed class NitroxInt3Converter() : ValueConverter<NitroxInt3, byte[]>(static id => Int3ToBytes(id),
                                                                                 static bytes => BytesToInt3(bytes))
{
    static byte[] Int3ToBytes(NitroxInt3 int3)
    {
        byte[] result = new byte[12];
        Array.Copy(BitConverter.GetBytes(int3.X), result, 4);
        Array.Copy(BitConverter.GetBytes(int3.Y), 0, result, 4, 4);
        Array.Copy(BitConverter.GetBytes(int3.Z), 0, result, 8, 4);
        return result;
    }

    static NitroxInt3 BytesToInt3(byte[] bytes)
    {
        NitroxInt3 result = default;
        result.X = BitConverter.ToInt32(bytes, 0);
        result.Y = BitConverter.ToInt32(bytes, 4);
        result.Z = BitConverter.ToInt32(bytes, 8);
        return result;
    }
}
