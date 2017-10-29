using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : AuthenticatedPacket
    {
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(string playerId, int type, int state) : base(playerId)
        {
            Type = type;
            State = state;
        }
    }
}
