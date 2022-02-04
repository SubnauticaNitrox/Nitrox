using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Exosuit_ArmSpawned_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Exosuit t) => t.UpdateColliders());

        public static void Postfix(Exosuit __instance)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().SpawnedArm(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

