using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class PlayerTeleported : Packet
{
    public SessionId SessionId { get; }
    public NitroxVector3 DestinationFrom { get; }
    public NitroxVector3 DestinationTo { get; }
    public Optional<NitroxId> SubRootID { get; }

    public PlayerTeleported(SessionId sessionId, NitroxVector3 destinationFrom, NitroxVector3 destinationTo, Optional<NitroxId> subRootID)
    {
        SessionId = sessionId;
        DestinationFrom = destinationFrom;
        DestinationTo = destinationTo;
        SubRootID = subRootID;
    }
}
