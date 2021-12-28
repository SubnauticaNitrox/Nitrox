using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsToggleEngineState : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool IsOn { get; protected set; }
        [Index(2)]
        public virtual bool IsStarting { get; protected set; }

        public CyclopsToggleEngineState() { }

        public CyclopsToggleEngineState(NitroxId id, bool isOn, bool isStarting)
        {
            Id = id;
            IsOn = isOn;
            IsStarting = isStarting;
        }

        public override string ToString()
        {
            return $"[CyclopsToggleEngineState - Id: {Id}, IsOn: {IsOn}, IsStarting: {IsStarting}]";
        }
    }
}
