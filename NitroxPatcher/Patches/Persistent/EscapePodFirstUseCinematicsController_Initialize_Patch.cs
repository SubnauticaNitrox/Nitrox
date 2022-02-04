using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    class EscapePodFirstUseCinematicsController_Initialize_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePodFirstUseCinematicsController t) => t.Initialize());

        public static bool Prefix(EscapePodFirstUseCinematicsController __instance)
        {
            __instance.bottomCinematicTarget.gameObject.SetActive(true);
            __instance.topCinematicTarget.gameObject.SetActive(true);

            __instance.bottomFirstUseCinematicTarget.gameObject.SetActive(false);
            __instance.topFirstUseCinematicTarget.gameObject.SetActive(false);

            return !Multiplayer.Active;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
