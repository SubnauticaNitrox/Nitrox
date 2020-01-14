using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using Harmony;
using ProtoBuf;
using NitroxClient.Helpers;

namespace NitroxPatcher.Patches.Persistent
{
    public class ProtobufSerializerPrecompiled_Deserialize_Patch : NitroxPatch
    {
        static Type TARGET_TYPE = typeof(ProtobufSerializer);
        static MethodInfo TARGET_METHOD = TARGET_TYPE.GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(Stream stream, object target, Type type)
        {
            int key;
            if (NitroxProtobufSerializer.Main.NitroxTypes.TryGetValue(type, out key))
            {
                NitroxProtobufSerializer.Main.Deserialize(stream, target, type);
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
