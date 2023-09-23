using System;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxModel;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.FMOD;

public class FMODSystem : FMODWhitelist
{
    private static readonly Type[] fmodPacketTypes = {
        typeof(FMODAssetPacket), typeof(FMODEventInstancePacket), typeof(FMODCustomEmitterPacket), typeof(FMODCustomLoopingEmitterPacket),
        typeof(FMODStudioEmitterPacket)
    };

    private readonly IPacketSender packetSender;

    public FMODSystem(IPacketSender packetSender) : base(GameInfo.Subnautica)
    {
        this.packetSender = packetSender;
    }

    /// <summary>
    /// Suppresses sounds played by base Subnautica, not any sounds triggered by Nitrox
    /// </summary>
    public static FMODSuppressor SuppressSubnauticaSounds()
    {
        return new FMODSuppressor();
    }

    public static MultiplePacketsSuppressor SuppressSendingSounds() => MultiplePacketsSuppressor.Suppress(fmodPacketTypes);

    /// <inheritdoc cref="FMODWhitelist.CalculateVolume"/>
    public static float CalculateVolume(Vector3 p1, Vector3 p2, float radius, float volume) => CalculateVolume(Vector3.Distance(p1, p2), radius, volume);

    public void SendAssetPlay(string path, NitroxVector3 position, float volume) => packetSender.Send(new FMODAssetPacket(path, position, volume));

    public void SendCustomEmitterPlay(NitroxId id, string assetPath) => packetSender.Send(new FMODCustomEmitterPacket(id, assetPath, true));
    public void SendCustomEmitterStop(NitroxId id, string assetPath) => packetSender.Send(new FMODCustomEmitterPacket(id, assetPath, false));

    public void SendCustomLoopingEmitterPlay(NitroxId id, string assetPath) => packetSender.Send(new FMODCustomLoopingEmitterPacket(id, assetPath));

    public void SendStudioEmitterPlay(NitroxId id, string assetPath, bool allowFadeout) => packetSender.Send(new FMODStudioEmitterPacket(id, assetPath, true, allowFadeout));
    public void SendStudioEmitterStop(NitroxId id, string assetPath, bool allowFadeout) => packetSender.Send(new FMODStudioEmitterPacket(id, assetPath, false, allowFadeout));

    public void SendEventInstancePlay(NitroxId id, string assetPath, NitroxVector3 position, float volume) => packetSender.Send(new FMODEventInstancePacket(id, true, assetPath, position, volume));
    public void SendEventInstanceStop(NitroxId id, string assetPath, NitroxVector3 position, float volume) => packetSender.Send(new FMODEventInstancePacket(id, false, assetPath, position, volume));
}
