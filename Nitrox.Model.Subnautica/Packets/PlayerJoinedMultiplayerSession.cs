using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerJoinedMultiplayerSession : Packet
{
    public PlayerContext PlayerContext { get; }
    public Optional<NitroxId> SubRootId { get; }
    public PlayerEntity PlayerEntity { get; }

    public PlayerJoinedMultiplayerSession(PlayerContext playerContext, Optional<NitroxId> subRootId, PlayerEntity playerEntity)
    {
        PlayerContext = playerContext;
        SubRootId = subRootId;
        PlayerEntity = playerEntity;
    }
}
