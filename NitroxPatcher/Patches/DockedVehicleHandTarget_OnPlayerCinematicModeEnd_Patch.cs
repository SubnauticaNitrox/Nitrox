using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Client
{
    class DockedVehicleHandTarget_OnPlayerCinematicModeEnd_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DockedVehicleHandTarget);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPlayerCinematicModeEnd", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(DockedVehicleHandTarget __instance, PlayerCinematicController cinematicController)
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            IMultiplayerSessionState multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSessionState>();

            Log.Debug($"{Time.realtimeSinceStartup} Cinematicmode ended! {cinematicController}");

            Vehicle dockedVehicle = __instance.dockingBay.GetDockedVehicle();
            Validate.NotNull(dockedVehicle);

            string vehicleGuid = GuidHelper.GetGuid(dockedVehicle.gameObject);
            string subRootGuid = GuidHelper.GetGuid(__instance.dockingBay.subRoot.gameObject);

            packetSender.Send(new VehicleDocking(
                multiplayerSession.Reservation.PlayerId,
                subRootGuid,
                vehicleGuid,
                DockingAction.UndockingPlayerIn));

            return true;
        }
        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
