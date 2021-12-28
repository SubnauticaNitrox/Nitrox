using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class AnimationChangeEvent : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual int Type { get; protected set; }
        [Index(2)]
        public virtual int State { get; protected set; }

        public AnimationChangeEvent() { }

        public AnimationChangeEvent(ushort playerId, int type, int state)
        {
            PlayerId = playerId;
            Type = type;
            State = state;
        }
    }
}
