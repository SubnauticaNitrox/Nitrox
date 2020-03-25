using System;
using System.Reflection;
using System.IO;
using Harmony;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Persistent
{
    public class ProtobufSerializer_Deserialize_Patch : NitroxPatch, IPersistentPatch
    {
        static Type TARGET_TYPE = typeof(ProtobufSerializer);
        static MethodInfo TARGET_METHOD = TARGET_TYPE.GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(Stream stream, object target, Type type)
        {
            int key;
            NitroxProtobufSerializer serializer = NitroxServiceLocator.LocateService<NitroxProtobufSerializer>();
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
