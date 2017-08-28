using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Client
{
    class DockedVehicleHandTarget_OnHandClick_Patch : NitroxPatch
    {
        // TODO: Check if all the OnHandClick/OnHandHover/UpdateValid code can be made more generic, to work for other objects that can be clicked to enter, such as PilotingChair, Vehicle and other CinematicModeTriggers.
        // Would only need a single abstract method to derive the right gameObject for the guid.
        // Would be nice to have multiple patches in a single class though.

        public static readonly Type TARGET_CLASS = typeof(DockedVehicleHandTarget);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(DockedVehicleHandTarget __instance, GUIHand hand)
        {
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            IMultiplayerSessionState multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSessionState>();

            Vehicle dockedVehicle = __instance.dockingBay.GetDockedVehicle();

            // isValidHandTarget is updated in a separate function, but is only read in this code path so it can safely be updated here.
            __instance.isValidHandTarget = dockedVehicle != null && simulationOwnership.CanClaimOwnership(GuidHelper.GetGuid(dockedVehicle.gameObject));

            if (__instance.isValidHandTarget)
            {
                string guid = GuidHelper.GetGuid(dockedVehicle.gameObject);
                string subGuid = GuidHelper.GetGuid(__instance.dockingBay.subRoot.gameObject);

                if (simulationOwnership.HasOwnership(guid))
                {
                    packetSender.Send(new VehicleDocking(multiplayerSession.Reservation.PlayerId, subGuid, guid, DockingAction.UndockingInitiate));
                    return true;
                }

                // Claim ownership, and once it's received call this function again.
                simulationOwnership.TryToRequestOwnership(guid, () => __instance.OnHandClick(hand));
                return false;
            }

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
