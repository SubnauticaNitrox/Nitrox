using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using ProtoBuf;
using NitroxClient.Helpers;

namespace NitroxPatcher.Patches.Persistent
{
    public class ProtobufSerializerPrecompiled_Serialize_Patch : NitroxPatch
    {
        static Type TARGET_TYPE = typeof(ProtobufSerializerPrecompiled);
        MethodInfo TARGET_METHOD = TARGET_TYPE.GetMethod("Serialize", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(int num, object obj, ProtoWriter writer)
        {
            if (num == int.MaxValue)
            {
                NitroxProtobufSerializer.Main.Serialize(writer, obj);
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
