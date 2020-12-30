using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PilotingChair_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PilotingChair);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        private static PilotingChair pilotingChair;
        private static GUIHand guiHand;
        private static bool skipPrefix = false;

        public static bool Prefix(PilotingChair __instance, GUIHand hand)
        {
            if (skipPrefix)
            {
                return true;
            }
            
            pilotingChair = __instance;
            guiHand = hand;

            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
            Validate.NotNull(subRoot, "PilotingChair cannot find it's corresponding SubRoot!");
            NitroxId id = NitroxEntity.GetId(subRoot.gameObject);

            if (simulationOwnership.HasExclusiveLock(id))
            {
                Log.Debug($"Already have an exclusive lock on the piloting chair: {id}");
                return true;
            }

            simulationOwnership.RequestSimulationLock(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse);

            return false;
        }

        private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired)
        {
            if (lockAquired)
            {
                skipPrefix = true;
                TARGET_METHOD.Invoke(pilotingChair, new[] { guiHand });
                skipPrefix = false;
            }
            else
            {
                pilotingChair.gameObject.AddComponent<DenyOwnershipHand>();
                pilotingChair.isValidHandTarget = false;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
