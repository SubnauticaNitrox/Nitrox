using System;
using System.IO;
using System.Reflection;
using Harmony;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Persistent
{
    public class ProtobufSerializer_Deserialize_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(ProtobufSerializer).GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly NitroxProtobufSerializer serializer = NitroxServiceLocator.LocateServicePreLifetime<NitroxProtobufSerializer>();

        public static bool Prefix(Stream stream, object target, Type type)
        {
            int key;
            if (Multiplayer.Active && serializer.NitroxTypes.TryGetValue(type, out key))
            {
                serializer.Deserialize(stream, target, type);
                return false;
            }

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
