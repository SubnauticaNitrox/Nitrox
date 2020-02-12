using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
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
            vehicles.AddVehicle(VehicleModelFactory.BuildFrom(packet));
            MethodInfo onCraftingBegin = typeof(Crafter).GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(onCraftingBegin);
            onCraftingBegin.Invoke(crafter, new object[] { packet.TechType.Enum(), packet.Duration }); //TODO: take into account latency for duration   

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.IsPresent())
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Get();
                NitroxEntity.SetNewId(constructedObject, packet.ConstructedItemId);
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenIds(constructedObject, packet.InteractiveChildIdentifiers);
            }
            else
            {
                Log2.Instance.LogMessage(NLogType.Error,"Could not find constructed object!");
            }
        }
    }
}
