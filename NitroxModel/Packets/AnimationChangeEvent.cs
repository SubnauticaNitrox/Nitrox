using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : AuthenticatedPacket
    {
        public int Type { get; set; }
        public int State { get; set; }

        public AnimationChangeEvent(string playerId, int type, int state) : base(playerId)
        {
            this.Type = type;
            this.State = state;
        }
    }
}
