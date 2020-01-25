using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsSonarButton_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsSonarButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsSonarButton __instance)
        {
            NitroxId id = NitroxIdentifier.GetId(__instance.subRoot.gameObject);
            bool activeSonar = Traverse.Create(__instance).Field("sonarActive").GetValue<bool>();
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeSonarState(id,activeSonar);
        }        

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
        
    }
}
