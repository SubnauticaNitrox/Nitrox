using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CreaturePositionsChangedProcessor : ClientPacketProcessor<CreaturePositionsChanged>
    {
        public override void Process(CreaturePositionsChanged creaturePositionsChanged)
        {
            foreach(var guidWithTransform in creaturePositionsChanged.GuidsWithTransform)
            {
                String guid = guidWithTransform.Key;
                Transform transform = guidWithTransform.Value;

                Optional<GameObject> creature = GuidHelper.GetObjectFrom(guid);

                if(creature.IsPresent())
                {
                    Log.Debug("Creature " + guid + " Moved!");
                    creature.Get().transform.position = transform.position;
                    creature.Get().transform.rotation = transform.rotation;
                    creature.Get().transform.localScale = transform.localScale;
                }
            }
        }
    }
}
