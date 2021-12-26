using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ToggleLights : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool IsOn { get; protected set; }

        private ToggleLights() { }

        public ToggleLights(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }
    }
}
