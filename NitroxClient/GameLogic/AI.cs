using NitroxClient.Communication;
using NitroxModel.GameLogic.Creatures.Actions;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class AI
    {
        private readonly IPacketSender packetSender;

        public AI(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void CreatureActionChanged(string guid, CreatureAction newAction)
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
                CreatureActionChanged actionChanged = new CreatureActionChanged(guid, creatureAction);
                packetSender.Send(actionChanged);
            }
        }
    }
}
