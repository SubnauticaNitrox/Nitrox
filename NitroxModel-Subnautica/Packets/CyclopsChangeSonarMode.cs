using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsChangeSonarMode : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool IsOn { get; protected set; }

        public CyclopsChangeSonarMode() { }

        public CyclopsChangeSonarMode(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return $"[CyclopsActivateSonar - Id: {Id}, IsOn: {IsOn}]";
        }
    }
}
