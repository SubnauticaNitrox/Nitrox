using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : Packet
    {
        public string PlayerId { get; }
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(string playerId, int type, int state)
        {
            PlayerId = playerId;
            Type = type;
            State = state;
        }
    }
}
