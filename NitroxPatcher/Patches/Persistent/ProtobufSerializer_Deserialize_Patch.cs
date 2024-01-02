using System;
using System.IO;
using System.Reflection;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public partial class ProtobufSerializer_Deserialize_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ProtobufSerializer t) => t.Deserialize(default(Stream), default(object), default(Type), default(bool)));
        private static readonly NitroxProtobufSerializer serializer = Resolve<NitroxProtobufSerializer>(true);

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
}
