using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : Packet
    {
        public ushort PlayerId { get; }
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(ushort playerId, int type, int state)
        {
            PlayerId = playerId;
            Type = type;
            State = state;
        }
    }
}
