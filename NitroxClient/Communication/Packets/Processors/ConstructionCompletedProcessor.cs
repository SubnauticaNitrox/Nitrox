using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructionCompletedProcessor : ClientPacketProcessor<ConstructionCompleted>
    {
        public override void Process(ConstructionCompleted completedPacket)
        {
            Console.WriteLine("Processing ConstructionCompleted " + completedPacket.Guid + " " + completedPacket.PlayerId + " " + completedPacket.NewBaseCreatedGuid);

            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(completedPacket.Guid);

            if(opGameObject.IsPresent())
            {
                GameObject constructing = opGameObject.Get();
                
                Constructable constructable = constructing.GetComponent<Constructable>();
                constructable.constructedAmount = 1f;
                constructable.SetState(true, true);

                if (completedPacket.NewBaseCreatedGuid.IsPresent())
                {
                    String newBaseGuid = completedPacket.NewBaseCreatedGuid.Get();
                    configureNewlyConstructedBase(newBaseGuid);
                }
            }
        }

        private void configureNewlyConstructedBase(String newBaseGuid)
        {
            Optional<object> opNewlyCreatedBase = TransientLocalObjectManager.Get(TransientLocalObjectManager.TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            if (opNewlyCreatedBase.IsPresent())
            {
                GameObject newlyCreatedBase = (GameObject)opNewlyCreatedBase.Get();
                GuidHelper.SetNewGuid(newlyCreatedBase, newBaseGuid);
            }
            else
            {
                Console.WriteLine("Could not assign new base guid as no newly constructed base was found");
            }
        }
    }
}
