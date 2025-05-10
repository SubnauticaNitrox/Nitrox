using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record PlayerTeleported(PeerId PlayerId, NitroxVector3 DestinationFrom, NitroxVector3 DestinationTo, Optional<NitroxId> SubRootID)
        : Packet
    {
        public PeerId PlayerId { get; } = PlayerId;
        public NitroxVector3 DestinationFrom { get; } = DestinationFrom;
        public NitroxVector3 DestinationTo { get; } = DestinationTo;
        public Optional<NitroxId> SubRootID { get; } = SubRootID;
    }
}
