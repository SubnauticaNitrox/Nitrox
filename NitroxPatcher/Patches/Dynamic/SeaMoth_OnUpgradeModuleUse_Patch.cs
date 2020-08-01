using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SeaMoth_OnUpgradeModuleUse_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(SeaMoth).GetMethod("OnUpgradeModuleUse", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(SeaMoth __instance, TechType techType, int slotID, out PacketSuppressor<ItemContainerRemove> __state)
        {
            __state = null;

            if (techType == TechType.SeamothElectricalDefense)
            {
                NitroxServiceLocator.LocateService<SeamothModulesEvent>().BroadcastElectricalDefense(techType, slotID, __instance);
            }
            else if (techType == TechType.SeamothTorpedoModule)
            {
                __state = NitroxServiceLocator.LocateService<IPacketSender>().Suppress<ItemContainerRemove>();
                NitroxServiceLocator.LocateService<SeamothModulesEvent>().BroadcastTorpedoLaunch(techType, slotID, __instance);
            }

            return true;
        }

        public static void Postfix(PacketSuppressor<ItemContainerRemove> __state)
        {
            __state?.Dispose();
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

