using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsToggleInternalLighting : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool IsOn { get; protected set; }

        public CyclopsToggleInternalLighting() { }

        public CyclopsToggleInternalLighting(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return $"[CyclopsToggleInternalLighting - Id: {Id}, IsOn: {IsOn}]";
        }
    }
}
