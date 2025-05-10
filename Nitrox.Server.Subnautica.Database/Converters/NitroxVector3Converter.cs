extern alias JB;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Database.Converters;

// TODO: Optimize (use different Sqlite data type?)
internal sealed class NitroxVector3Converter() : ValueConverter<NitroxVector3, byte[]>(static id => Int3ToBytes(id),
                                                                                 static bytes => BytesToInt3(bytes))
{
    static byte[] Int3ToBytes(NitroxVector3 vec3)
    {
        byte[] result = new byte[12];
        Array.Copy(BitConverter.GetBytes(vec3.X), result, 4);
        Array.Copy(BitConverter.GetBytes(vec3.Y), 0, result, 4, 4);
        Array.Copy(BitConverter.GetBytes(vec3.Z), 0, result, 8, 4);
        return result;
    }

    static NitroxVector3 BytesToInt3(byte[] bytes)
    {
        NitroxVector3 result = default;
        result.X = BitConverter.ToSingle(bytes, 0);
        result.Y = BitConverter.ToSingle(bytes, 4);
        result.Z = BitConverter.ToSingle(bytes, 8);
        return result;
    }
}
