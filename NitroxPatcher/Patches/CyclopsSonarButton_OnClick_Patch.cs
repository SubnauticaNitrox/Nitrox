using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    class CyclopsSonarButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsSonarButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsSonarButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().ActivateSonar(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
