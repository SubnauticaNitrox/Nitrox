﻿using Harmony;
using System.Reflection;

namespace NitroxPatcher.Patches.Persistent
{
    public class PrefabPlaceholdersGroup_Spawn_Patch : NitroxPatch
    {
        public static MethodInfo TARGET_METHOD = typeof(PrefabPlaceholdersGroup).GetMethod("Spawn", BindingFlags.Instance | BindingFlags.Public);

        public static bool Prefix()
        {
            return false; // Disable spawning of PrefabPlaceholders(In other words large portion of objects)
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
