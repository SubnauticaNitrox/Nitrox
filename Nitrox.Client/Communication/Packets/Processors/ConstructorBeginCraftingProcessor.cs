using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using static Nitrox.Client.GameLogic.Helper.TransientLocalObjectManager;

namespace Nitrox.Client.Communication.Packets.Processors
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
            crafter.ReflectionCall("OnCraftingBegin", false, false, new object[] { packet.VehicleModel.TechType.ToUnity(), packet.Duration });

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
