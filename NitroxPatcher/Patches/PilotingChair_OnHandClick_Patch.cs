using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class PilotingChair_OnHandClick_Patch : NitroxPatch
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
            string guid = GuidHelper.GetGuid(subRoot.gameObject);

            if (simulationOwnership.HasExclusiveLock(guid))
            {
                StaticLogger.Instance.Debug($"Already have an exclusive lock on the piloting chair: {guid}");
                return true;
            }

            simulationOwnership.RequestSimulationLock(guid, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse);

            return false;
        }

        private static void ReceivedSimulationLockResponse(string guid, bool lockAquired)
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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
