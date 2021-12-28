using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsChangeEngineMode : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual CyclopsMotorMode.CyclopsMotorModes Mode { get; protected set; }

        public CyclopsChangeEngineMode() { }

        public CyclopsChangeEngineMode(NitroxId id, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            Id = id;
            Mode = mode;
        }

        public override string ToString()
        {
            return $"[CyclopsChangeEngineMode - Id: {Id}, Mode: {Mode}]";
        }
    }
}
