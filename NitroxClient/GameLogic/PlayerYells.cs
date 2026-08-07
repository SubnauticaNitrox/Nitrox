using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.GameLogic;

public sealed class PlayerYells(
    IPacketSender packetSender,
    LocalPlayer localPlayer,
    PlayerManager playerManager,
    SeamothPassengers seamothPassengers,
    PlayerYellSound playerYellSound)
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerManager playerManager = playerManager;
    private readonly PlayerYellSound playerYellSound = playerYellSound;
    private readonly SeamothPassengers seamothPassengers = seamothPassengers;

    public PlayerEmoteGroup RecentGroup { get; private set; } = PlayerEmoteGroup.Yes;

    public bool CanYell()
    {
        Player player = Player.main;
        return player && localPlayer.SessionId.HasValue && TryGetYellContext(player, out _);
    }

    public bool TryYell() => TryYellRecent();

    public bool TryYellRecent() => TryYell(RecentGroup);

    public bool TryYell(PlayerEmoteGroup group)
    {
        Player player = Player.main;
        if (!player || !localPlayer.SessionId.HasValue || !TryGetYellContext(player, out bool isInsideVehicle))
        {
            return false;
        }

        PlayerEmoteDefinition definition = PlayerEmoteCatalog.Get(group);
        SessionId sessionId = localPlayer.SessionId.Value;
        byte soundIndex = definition.SoundIndices[UnityEngine.Random.Range(0, definition.SoundIndices.Count)];
        GameObject source = Player.mainObject ? Player.mainObject : player.gameObject;

        playerYellSound.TryPlay(sessionId, source, soundIndex, !isInsideVehicle);
        packetSender.Send(new PlayerYell(sessionId, soundIndex, isInsideVehicle));
        RecentGroup = group;
        return true;
    }

    public void PlayRemote(PlayerYell packet)
    {
        if (packet.SoundIndex >= PlayerYell.SOUND_COUNT)
        {
            Log.Warn($"[{nameof(PlayerYells)}] Ignoring invalid yell sound index {packet.SoundIndex} from player {packet.SessionId}.");
            return;
        }

        if (!playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer) || !remotePlayer.Body)
        {
            Log.Warn($"[{nameof(PlayerYells)}] Could not find the player body for yell from {packet.SessionId}.");
            return;
        }

        playerYellSound.TryPlay(packet.SessionId, remotePlayer.Body, packet.SoundIndex, !packet.IsInsideVehicle);
    }

    private bool TryGetYellContext(Player player, out bool isInsideVehicle)
    {
        SubRoot currentSub = player.currentSub;
        bool isInsideCyclops = currentSub && currentSub.isCyclops;
        if (player.currentMountedVehicle || (isInsideCyclops && player.mode == Player.Mode.Piloting))
        {
            isInsideVehicle = false;
            return false;
        }

        isInsideVehicle = player.inSeamoth || seamothPassengers.IsPassenger || isInsideCyclops;
        return true;
    }
}
