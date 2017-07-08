using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic.ManagedObjects;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructorBeginCraftingProcessor : GenericPacketProcessor<ConstructorBeginCrafting>
    {
        private MultiplayerObjectManager multiplayerObjectManager;

        public ConstructorBeginCraftingProcessor(MultiplayerObjectManager multiplayerObjectManager)
        {
            this.multiplayerObjectManager = multiplayerObjectManager;
        }

        public override void Process(ConstructorBeginCrafting packet)
        {
            Optional<GameObject> opGameObject = multiplayerObjectManager.GetManagedObject(packet.Guid);

            if(opGameObject.IsEmpty())
            {
                Console.WriteLine("Trying to build " + packet.TechType + " with unmanaged constructor - ignoring.");
                return;
            }

            GameObject gameObject = opGameObject.Get();
            Crafter crafter = gameObject.GetComponent<Crafter>();

            if(crafter == null)
            {
                Console.WriteLine("Trying to build " + packet.TechType + " but we did not have a corresponding crafter - how did that happen?");
                return;
            }

            Optional<TechType> opTechType = ApiHelper.TechType(packet.TechType);

            if(opTechType.IsEmpty())
            {
                Console.WriteLine("Trying to build unknown tech type: " + packet.TechType + " - ignoring.");
                return;
            }

            MethodInfo onCraftingBegin = this.GetType().GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);
            onCraftingBegin.Invoke(crafter, new object[] { opTechType.Get(), packet.Duration }); //TODO: take into account latency for duration            
        }
    }
}
