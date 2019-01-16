using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleUndockingProcessor : ClientPacketProcessor<VehicleUndocking>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleUndockingProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleUndocking packet)
        {
            GameObject vehicleGo = GuidHelper.RequireObjectFrom(packet.VehicleGuid);
            GameObject vehicleDockingBayGo = GuidHelper.RequireObjectFrom(packet.DockGuid);

            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponentInChildren<VehicleDockingBay>();
            
            using (packetSender.Suppress<VehicleUndocking>())
            {
                vehicles.SetOnPilotMode(packet.VehicleGuid, packet.PlayerId, true);
                vehicleDockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
                SkyEnvironmentChanged.Broadcast(vehicleGo, (GameObject)null);
                vehicleDockingBay.ReflectionSet("_dockedVehicle", null);
                vehicle.docked = false;
                vehicle.useRigidbody.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
            }
        }
    }
}
