using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System.Collections;
using UnityEngine;
using NitroxClient.MonoBehaviours;

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
            GameObject vehicleGo = NitroxEntity.RequireObjectFrom(packet.VehicleId);
            GameObject vehicleDockingBayGo = NitroxEntity.RequireObjectFrom(packet.DockId);

            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponentInChildren<VehicleDockingBay>();
            
            using (packetSender.Suppress<VehicleUndocking>())
            {
                vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
                vehicleDockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
                SkyEnvironmentChanged.Broadcast(vehicleGo, (GameObject)null);

                

                vehicle.StartCoroutine(WaitBeforePushDown(vehicle, vehicleDockingBay));
                
            }
        }

        IEnumerator WaitBeforePushDown(Vehicle vehicle, VehicleDockingBay vehicleDockingBay)
        {
            yield return new WaitForSeconds(6.0f);
            
            vehicleDockingBay.SetVehicleUndocked();
            vehicleDockingBay.ReflectionSet("vehicle_docked_param", false);
            vehicleDockingBay.ReflectionSet("_dockedVehicle", null);
            vehicle.docked = false;
            vehicle.useRigidbody.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
        }
    }
}
