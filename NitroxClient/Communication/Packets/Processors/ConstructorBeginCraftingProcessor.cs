using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructorBeginCraftingProcessor : ClientPacketProcessor<ConstructorBeginCrafting>
    {
        public static GameObject ConstructedObject;

        private readonly Vehicles vehicles;

        public ConstructorBeginCraftingProcessor(Vehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void Process(ConstructorBeginCrafting packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.ConstructorId);
            Crafter crafter = gameObject.RequireComponentInChildren<Crafter>(true);
            crafter.OnCraftingBegin(packet.VehicleModel.TechType.ToUnity(), packet.Duration);

            vehicles.AddVehicle(packet.VehicleModel);

            Optional<object> opConstructedObject = Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.HasValue)
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Value;
                NitroxEntity.SetNewId(constructedObject, packet.VehicleModel.Id);
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(constructedObject, packet.VehicleModel.InteractiveChildIdentifiers);
            }
            else
            {
                Log.Error($"Could not find constructed object {packet.VehicleModel.Id} from constructor {packet.ConstructorId}");
            }
        }
    }
}
