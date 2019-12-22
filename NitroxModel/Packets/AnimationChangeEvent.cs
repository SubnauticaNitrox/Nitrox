using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : Packet
    {
        public NitroxId PlayerId { get; }
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(NitroxId playerId, int type, int state)
        {
            PlayerId = playerId;
            Type = type;
            State = state;
        }
    }
}
