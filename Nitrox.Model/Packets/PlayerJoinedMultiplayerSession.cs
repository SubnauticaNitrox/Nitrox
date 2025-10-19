using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Model.DataStructures.Util;
using Nitrox.Model.MultiplayerSession;

namespace Nitrox.Model.Packets;

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
