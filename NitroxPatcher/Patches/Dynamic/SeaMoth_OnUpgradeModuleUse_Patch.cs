using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SeaMoth_OnUpgradeModuleUse_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaMoth t) => t.OnUpgradeModuleUse(default(TechType), default(int)));

        public static bool Prefix(SeaMoth __instance, TechType techType, int slotID, out PacketSuppressor<ItemContainerRemove> __state)
        {
            __state = null;

            switch (techType)
            {
                case TechType.SeamothElectricalDefense:
                    NitroxServiceLocator.LocateService<SeamothModulesEvent>().BroadcastElectricalDefense(techType, slotID, __instance);
                    break;
                case TechType.SeamothTorpedoModule:
                    __state = NitroxServiceLocator.LocateService<IPacketSender>().Suppress<ItemContainerRemove>();
                    NitroxServiceLocator.LocateService<SeamothModulesEvent>().BroadcastTorpedoLaunch(techType, slotID, __instance);
                    break;
            }

            return true;
        }

        public static void Postfix(PacketSuppressor<ItemContainerRemove> __state)
        {
            __state?.Dispose();
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}

