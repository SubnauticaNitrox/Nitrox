using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerJoinedMultiplayerSession : Packet
{
    public PlayerContext PlayerContext { get; }
    public Optional<NitroxId> SubRootId { get; }
    public PlayerWorldEntity PlayerWorldEntity { get; }

    public PlayerJoinedMultiplayerSession(PlayerContext playerContext, Optional<NitroxId> subRootId, PlayerWorldEntity playerWorldEntity)
    {
        PlayerContext = playerContext;
        SubRootId = subRootId;
        PlayerWorldEntity = playerWorldEntity;
    }
}
