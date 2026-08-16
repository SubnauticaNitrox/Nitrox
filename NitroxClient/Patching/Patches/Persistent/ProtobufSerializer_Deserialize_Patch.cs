using System;
using System.IO;
using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxClient.Services;

namespace NitroxClient.Patching.Patches.Persistent;

internal sealed partial class ProtobufSerializer_Deserialize_Patch : NitroxPatch, IPersistentPatch
{
    private static NitroxProtobufSerializerService serializer;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ProtobufSerializer t) => t.Deserialize(default(Stream), default(object), default(Type), default(bool)));

    public ProtobufSerializer_Deserialize_Patch(NitroxProtobufSerializerService serializer)
    {
        ProtobufSerializer_Deserialize_Patch.serializer = serializer;
    }

    public static bool Prefix(Stream stream, object target, Type type, bool verbose)
    {
        if (Multiplayer.Active && serializer.NitroxTypes.ContainsKey(type))
        {
            serializer.Deserialize(stream, target, type);
            return false;
        }

        return true;
    }
}
