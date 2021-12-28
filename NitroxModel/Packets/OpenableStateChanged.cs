using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class OpenableStateChanged : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool IsOpen { get; protected set; }
        [Index(2)]
        public virtual float Duration { get; protected set; }

        public OpenableStateChanged() { }

        public OpenableStateChanged(NitroxId id, bool isOpen, float duration)
        {
            Id = id;
            IsOpen = isOpen;
            Duration = duration;
        }
    }
}
