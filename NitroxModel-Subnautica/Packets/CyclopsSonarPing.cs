using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsSonarPing : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        private CyclopsSonarPing() { }

        public CyclopsSonarPing(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[CyclopsSonarPing - Id: {Id}]";
        }
    }
}
