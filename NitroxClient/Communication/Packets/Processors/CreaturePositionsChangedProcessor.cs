using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CreaturePositionsChangedProcessor : ClientPacketProcessor<CreaturePositionsChanged>
    {
        public override void Process(CreaturePositionsChanged creaturePositionsChanged)
        {
            foreach(String guid in creaturePositionsChanged.Guids)
            {
                Optional<GameObject> creature = GuidHelper.GetObjectFrom(guid);

                if(creature.IsPresent())
                {
                    Console.WriteLine("Creature " + guid + " Moved!");
                    creaturePositionsChanged.SetTransform(creature.Get().transform, guid);
                }
            }
        }
    }
}
