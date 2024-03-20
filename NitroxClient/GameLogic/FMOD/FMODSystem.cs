using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.FMOD;

public class FMODSystem
{
    private readonly IPacketSender packetSender;

    public FMODSystem(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    /// <inheritdoc cref="FMODSoundSuppressor"/>
    public static FMODSoundSuppressor SuppressSubnauticaSounds() => new();

    /// <summary>
    /// Suppresses sending any sound packet
    /// </summary>
    public static PacketSuppressor<FMODAssetPacket, FMODEventInstancePacket, FMODCustomEmitterPacket, FMODCustomLoopingEmitterPacket, FMODStudioEmitterPacket> SuppressSendingSounds()
    {
        return PacketSuppressor<FMODAssetPacket, FMODEventInstancePacket, FMODCustomEmitterPacket, FMODCustomLoopingEmitterPacket, FMODStudioEmitterPacket>.Suppress();
    }

    /// <inheritdoc cref="SoundHelper.CalculateVolume"/>
    public static float CalculateVolume(Vector3 p1, Vector3 p2, float radius, float volume) => SoundHelper.CalculateVolume(Vector3.Distance(p1, p2), radius, volume);

    public void SendAssetPlay(string path, NitroxVector3 position, float volume) => packetSender.Send(new FMODAssetPacket(path, position, volume));

    public void SendCustomEmitterPlay(NitroxId id, string assetPath) => packetSender.Send(new FMODCustomEmitterPacket(id, assetPath, true));
    public void SendCustomEmitterStop(NitroxId id, string assetPath) => packetSender.Send(new FMODCustomEmitterPacket(id, assetPath, false));

    public void SendCustomLoopingEmitterPlay(NitroxId id, string assetPath) => packetSender.Send(new FMODCustomLoopingEmitterPacket(id, assetPath));

    public void SendStudioEmitterPlay(NitroxId id, string assetPath, bool allowFadeout) => packetSender.Send(new FMODStudioEmitterPacket(id, assetPath, true, allowFadeout));
    public void SendStudioEmitterStop(NitroxId id, string assetPath, bool allowFadeout) => packetSender.Send(new FMODStudioEmitterPacket(id, assetPath, false, allowFadeout));

    public void SendEventInstancePlay(NitroxId id, string assetPath, NitroxVector3 position, float volume) => packetSender.Send(new FMODEventInstancePacket(id, true, assetPath, position, volume));
    public void SendEventInstanceStop(NitroxId id, string assetPath, NitroxVector3 position, float volume) => packetSender.Send(new FMODEventInstancePacket(id, false, assetPath, position, volume));
}
