using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Util;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
    {
        public ushort PlayerId { get; }
        public Optional<NitroxId> SubRootId { get; }

        public SubRootChanged(ushort playerId, Optional<NitroxId> subRootId)
        {
            PlayerId = playerId;
            SubRootId = subRootId;
        }
    }
}
