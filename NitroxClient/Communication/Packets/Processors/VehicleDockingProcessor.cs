using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class VehicleDockingProcessor : ClientPacketProcessor<VehicleDocking>
    {
        // TODO: Properly analyze and consider redundancy:
        // A VehicleMovementPacket means:
        // In case of a Vehicle (SeaMoth/Exosuit) that it exists, it's undocked (add this check in the fixedupdate sender?) and the player is inside.
        // For a Cyclops it needs to exist and the player should be in the PilotingChair.

        // So the final question is: what do we do with such kind of redundancies? If there are specialized packets for things, then we can either do nothing or add the redundant checks anyway, just in case. Or we can get rid of the specialized packets, and do it all in the other packetprocessors, but this will burden the class and logic. This should probably be determined case-by-case, at some point a decision should just be made and if it turns out it doesn't work or isn't obvious enough, it can always be refactored.

        /*
        44.89321 VehicleDockingBay.set_dockedVehicle: SeaMoth(Clone) (SeaMoth)
        52.02565 GUIHand.Send
        52.02607 DockedVehicleHandTarget.OnPlayerCinematicModeStart
        52.0261 VehicleDockingBay.set_dockedVehicle: SeaMoth(Clone) (SeaMoth)
        52.02763 VehicleDockingBay.OnUndockingStart
        Cinematicmode ended!UndockCinematic(PlayerCinematicController)
        56.99169 PlayerCinematicController.OnPlayerCinematicModeEnd
        56.9919 Vehicle.OnUndockingComplete
        56.99794 VehicleDockingBay.OnUndockingComplete
        56.99816 VehicleDockingBay.set_dockedVehicle:
        */

        public override void Process(VehicleDocking packet)
        {
            Log.Debug($"{Time.realtimeSinceStartup} received docking packet:\n\t{packet}");

            Optional<GameObject> opSubRoot = GuidHelper.GetObjectFrom(packet.SubRootGuid);
            Validate.IsPresent(opSubRoot, "No SubRoot found for: " + packet.SubRootGuid);

            var subRoot = opSubRoot.Get();

            var vdb = subRoot.GetComponentInChildren<VehicleDockingBay>();
            Validate.NotNull(vdb, "VehicleDockingBay is not in subroot children!");

            Optional<GameObject> opVehicle = GuidHelper.GetObjectFrom(packet.VehicleGuid);
            Validate.IsPresent(opVehicle, "No Vehicle found for: " + packet.VehicleGuid);

            var vehicle = opVehicle.Get().GetComponent<Vehicle>();

            switch (packet.DockingAction)
            {
                case DockingAction.Docking:
                    vdb.DockVehicle(vehicle);
                    break;
                case DockingAction.UndockingInitiate:
                    // For some reason the undocking sequence in DockedVehicleHandTarget.OnPlayerCinematicModeStart starts off with docking
                    vdb.DockVehicle(vehicle);
                    // Then OnUndockingStart is fired from DockedVehicleHandTarget.OnStartCinematicMode.
                    vdb.OnUndockingStart();

                    // TODO: Figure out why the "vehicle ejection animation" starts immediately, instead of waiting for the player to enter the vehicle. These function calls are the same as on the local game.
                    break;
                case DockingAction.UndockingPlayerIn:
                    Log.Debug("Player entering vehicle should be finished, throwing vehicle out");
                    // Done by DVHT.OnPlayerCinematicModeEnd -> Vehicle.OnUndockingComplete (but this also does thing to the Player, so it's not called directly):
                    vehicle.docked = false;
                    // DVHT.OnPlayerCinematicModeEnd:
                    vdb.OnUndockingComplete(null);

                    // TODO: Disconnect RemotePlayer from sub, when ParentChange gets deprecated?
                    break;
            }
        }
    }
}
