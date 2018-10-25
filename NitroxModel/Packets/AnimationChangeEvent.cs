using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : Packet
    {
        public ulong PlayerId { get; }
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(ulong playerId, int type, int state)
        {
            PlayerId = playerId;
            Type = type;
            State = state;
        }
    }
}
