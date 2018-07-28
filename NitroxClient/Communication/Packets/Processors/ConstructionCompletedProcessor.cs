﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructionCompletedProcessor : ClientPacketProcessor<ConstructionCompleted>
    {
        public override void Process(ConstructionCompleted completedPacket)
        {
            Log.Debug("Processing ConstructionCompleted " + completedPacket.Guid + " " + completedPacket.NewBaseCreatedGuid);

            GameObject constructing = GuidHelper.RequireObjectFrom(completedPacket.Guid);            
            Constructable constructable = constructing.GetComponent<Constructable>();
            constructable.constructedAmount = 1f;
            constructable.SetState(true, true);

            if (completedPacket.NewBaseCreatedGuid.IsPresent())
            {
                string newBaseGuid = completedPacket.NewBaseCreatedGuid.Get();
                ConfigureNewlyConstructedBase(newBaseGuid);
            }            
        }

        private void ConfigureNewlyConstructedBase(string newBaseGuid)
        {
            Optional<object> opNewlyCreatedBase = TransientLocalObjectManager.Get(TransientLocalObjectManager.TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            if (opNewlyCreatedBase.IsPresent())
            {
                GameObject newlyCreatedBase = (GameObject)opNewlyCreatedBase.Get();
                GuidHelper.SetNewGuid(newlyCreatedBase, newBaseGuid);
            }
            else
            {
                Log.Error("Could not assign new base guid as no newly constructed base was found");
            }
        }
    }
}
