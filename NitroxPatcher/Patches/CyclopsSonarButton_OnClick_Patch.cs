using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    class CyclopsSonarButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsSonarButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsSonarButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            bool activeSonar = Traverse.Create(__instance).Field("sonarActive").GetValue<bool>();
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeSonarState(guid,activeSonar);
        }        

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
        
    }
}
