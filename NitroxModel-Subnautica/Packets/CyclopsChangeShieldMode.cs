using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsChangeShieldMode : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool IsOn { get; protected set; }

        public CyclopsChangeShieldMode() { }

        public CyclopsChangeShieldMode(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return $"[CyclopsActivateShield - Id: {Id}, IsOn: {IsOn}]";
        }
    }
}
