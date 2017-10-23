using NitroxClient.Communication;
using NitroxModel.GameLogic.Creatures.Actions;
using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic
{
    public class AI
    {
        private PacketSender packetSender;

        public AI(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void CreatureActionChanged(String guid, CreatureAction newAction)
        {
            SerializableCreatureAction creatureAction = null;

            if (newAction.GetType() == typeof(SwimToPoint))
            {
                creatureAction = new SwimToPointAction(((SwimToPoint)newAction).Target);
            }

            if(creatureAction != null)
            {
                CreatureActionChanged actionChanged = new CreatureActionChanged(guid, creatureAction, packetSender.PlayerId);
                packetSender.Send(actionChanged);
            }
        } 
    }
}
