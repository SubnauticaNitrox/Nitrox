using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : Packet
    {
        public ulong LPlayerId { get; }
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(ulong playerId, int type, int state)
        {
            LPlayerId = playerId;
            Type = type;
            State = state;
        }
    }
}
