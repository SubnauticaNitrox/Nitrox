using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class EscapePodFirstUseCinematicsController_Initialize_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(EscapePodFirstUseCinematicsController).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(EscapePodFirstUseCinematicsController __instance)
        {
            __instance.bottomCinematicTarget.gameObject.SetActive(true);
            __instance.topCinematicTarget.gameObject.SetActive(true);

            __instance.bottomFirstUseCinematicTarget.gameObject.SetActive(false);
            __instance.topFirstUseCinematicTarget.gameObject.SetActive(false);
            
            return !Multiplayer.Active;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
