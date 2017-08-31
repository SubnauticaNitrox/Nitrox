using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
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
        private PlayerManager remotePlayerManager;

        public VehicleDockingProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        // TODO: Properly analyze and consider redundancy with VehicleMovement packets, as these mean (in case of a Seamoth/ExoSuit) that they are undocked.

        public override void Process(VehicleDocking packet)
        {
            Log.Debug($"{Time.realtimeSinceStartup} received docking packet:\n\t{packet}");

            Optional<GameObject> opSubRoot = GuidHelper.GetObjectFrom(packet.SubRootGuid);
            Validate.IsPresent(opSubRoot, "No SubRoot found for: " + packet.SubRootGuid);

            var subRootGameObj = opSubRoot.Get();
            var subRoot = subRootGameObj.GetComponent<SubRoot>();

            var vdb = subRootGameObj.GetComponentInChildren<VehicleDockingBay>();
            Validate.NotNull(vdb, "VehicleDockingBay is not in subroot children!");

            Optional<GameObject> opVehicle = GuidHelper.GetObjectFrom(packet.VehicleGuid);
            Validate.IsPresent(opVehicle, "No Vehicle found for: " + packet.VehicleGuid);

            var vehicle = opVehicle.Get().GetComponent<Vehicle>();

            var opPlayer = remotePlayerManager.Find(packet.PlayerId);
            Validate.IsPresent(opPlayer);

            var player = opPlayer.Get();

            switch (packet.DockingAction)
            {
                case DockingAction.Docking:
                    vdb.DockVehicle(vehicle);

                    player.SetVehicle(null);
                    player.SetSubRoot(subRoot);
                    break;
                case DockingAction.UndockingInitiate:
                    // TODO: Figure out at which stage this should happen.
                    player.SetSubRoot(null);
                    player.SetVehicle(vehicle);

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
                    // Done by DVHT.OnPlayerCinematicModeEnd -> Vehicle.OnUndockingComplete (but this also does things to the Player, so it's not called directly):
                    vehicle.docked = false;
                    player.SetSubRoot(null);
                    player.SetVehicle(vehicle);
                    break;
            }
        }
    }
}
