using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class SeaMoth_OnUpgradeModuleUse_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SeaMoth);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnUpgradeModuleUse", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(SeaMoth __instance, TechType techType, int slotID, PacketSuppressor<ItemContainerRemove> __state)
        {
            
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
            if (__state != null)
            {
                __state.Dispose();
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}

