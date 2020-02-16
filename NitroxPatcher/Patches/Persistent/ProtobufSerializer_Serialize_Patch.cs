using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using ProtoBuf;
using System.IO;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class ProtobufSerializer_Serialize_Patch : NitroxPatch, IPersistentPatch
    {
        static Type TARGET_TYPE = typeof(ProtobufSerializer);
        static MethodInfo TARGET_METHOD = TARGET_TYPE.GetMethod("Serialize", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(Stream stream, object source, Type type)
        {
            int key;
            if (Multiplayer.Active && NitroxProtobufSerializer.Main.NitroxTypes.TryGetValue(type, out key))
            {
                NitroxProtobufSerializer.Main.Serialize(stream, source);
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
