using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Client
{
    class DockedVehicleHandTarget_OnHandHover_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DockedVehicleHandTarget);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandHover", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(DockedVehicleHandTarget __instance, GUIHand hand)
        {
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            // Handle the case where another player is in the vehicle, playing the animation (as otherwise the player is not in the vehicle or the vehicle is not docked).
            // This patch is only to update the text and icon.

            Vehicle dockedVehicle = __instance.dockingBay.GetDockedVehicle();
            if (dockedVehicle)
            {
                // TODO: Check isValidHandTarget.
                // Also, PilotingChair for instance updates it locally...

                string guid = GuidHelper.GetGuid(dockedVehicle.gameObject);
                if (!simulationOwnership.CanClaimOwnership(guid))
                {
                    // TODO: Localized strings?
                    HandReticle.main.SetInteractText("Another player is currently in this vehicle!");
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                    return false;
                }
            }
            return true;
        }
        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
