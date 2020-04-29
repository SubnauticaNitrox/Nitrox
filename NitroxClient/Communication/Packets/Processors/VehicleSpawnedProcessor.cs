using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleSpawnedProcessor : ClientPacketProcessor<VehicleSpawned>
    {
        private readonly Vehicles vehicles;

        public VehicleSpawnedProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void Process(VehicleSpawned packet)
        {
            vehicles.AddVehicle(packet.VehicleModel);

            GameObject gameObject = SerializationHelper.GetGameObject(packet.SerializedData);
            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));
            gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
        }
    }
}
