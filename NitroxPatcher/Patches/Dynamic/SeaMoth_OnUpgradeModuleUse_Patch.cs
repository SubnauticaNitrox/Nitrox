using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SeaMoth_OnUpgradeModuleUse_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaMoth t) => t.OnUpgradeModuleUse(default(TechType), default(int)));

        public static bool Prefix(SeaMoth __instance, TechType techType, int slotID)
        {
            switch (techType)
            {
                case TechType.SeamothElectricalDefense:
                    Resolve<SeamothModulesEvent>().BroadcastElectricalDefense(techType, slotID, __instance);
                    break;
                case TechType.SeamothTorpedoModule:
                    Resolve<SeamothModulesEvent>().BroadcastTorpedoLaunch(techType, slotID, __instance);
                    break;
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true);
        }
    }
}

