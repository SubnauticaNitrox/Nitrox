using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Creatures.Actions;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Client.GameLogic
{
    public class AI
    {
        private readonly IPacketSender packetSender;

        public AI(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void CreatureActionChanged(NitroxId id, CreatureAction newAction)
        {
            SerializableCreatureAction creatureAction = null;

            /*
            Example for next implementation:

            if (newAction.GetType() == typeof(SwimToPoint))
            {
                creatureAction = new SwimToPointAction(((SwimToPoint)newAction).Target);
            }*/

            if (creatureAction != null)
            {
                CreatureActionChanged actionChanged = new CreatureActionChanged(id, creatureAction);
                packetSender.Send(actionChanged);
            }
        }
    }
}
