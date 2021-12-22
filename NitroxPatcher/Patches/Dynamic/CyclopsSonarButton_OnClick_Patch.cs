using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsSonarButton_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarButton t) => t.OnClick());

        public static void Postfix(CyclopsSonarButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            Resolve<Cyclops>().BroadcastChangeSonarState(id, __instance.sonarActive);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }

    }
}
