using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructorBeginCraftingProcessor : ClientPacketProcessor<ConstructorBeginCrafting>
    {
        public static GameObject ConstructedObject;

        private readonly INitroxLogger log;

        public ConstructorBeginCraftingProcessor(INitroxLogger logger)
        {
            log = logger;
        }

        public override void Process(ConstructorBeginCrafting packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.ConstructorGuid);
            Crafter crafter = gameObject.RequireComponentInChildren<Crafter>(true);

            MethodInfo onCraftingBegin = typeof(Crafter).GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(onCraftingBegin);
            onCraftingBegin.Invoke(crafter, new object[] { packet.TechType, packet.Duration }); //TODO: take into account latency for duration   

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.IsPresent())
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Get();
                GuidHelper.SetNewGuid(constructedObject, packet.ConstructedItemGuid);
                VehicleChildObjectIdentifierHelper.SetInteractiveChildrenGuids(constructedObject, packet.InteractiveChildIdentifiers);
            }
            else
            {
                log.Error("Could not find constructed object!");
            }
        }
    }
}
