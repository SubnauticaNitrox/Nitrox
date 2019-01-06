using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AnimationChangeEvent : Packet
    {
        public PlayerContext PlayerContext;
        public int Type { get; }
        public int State { get; }

        public AnimationChangeEvent(PlayerContext playerContext, int type, int state)
        {
            PlayerContext = playerContext;
            Type = type;
            State = state;
        }
    }
}
