using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : Packet
    {
        public NitroxId PieceId { get; }
        public NitroxId BaseId { get; }
        public Optional<NitroxId> BypassExistingNitroxId { get; }

        public ConstructionCompleted(NitroxId id, NitroxId baseId, Optional<NitroxId> bypassExistingNitroxId)
        {
            PieceId = id;
            BaseId = baseId;
            BypassExistingNitroxId = bypassExistingNitroxId;
        }
    }
}
