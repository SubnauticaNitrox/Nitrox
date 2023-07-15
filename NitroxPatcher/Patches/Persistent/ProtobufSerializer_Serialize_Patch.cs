using System;
using System.IO;
using System.Reflection;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public partial class ProtobufSerializer_Serialize_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ProtobufSerializer t) => t.Serialize(default(Stream), default(object), default(Type)));

        /// <summary>
        ///     This patch is in a hot path so it needs this optimization.
        /// </summary>
        private static readonly NitroxProtobufSerializer serializer = Resolve<NitroxProtobufSerializer>(true);

        public static bool Prefix(Stream stream, object source, Type type)
        {
            if (Multiplayer.Active && serializer.NitroxTypes.ContainsKey(type))
            {
                serializer.Serialize(stream, source);
                return false;
            }

            return true;
        }
    }
}
