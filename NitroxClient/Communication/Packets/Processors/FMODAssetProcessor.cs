using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FMODAssetProcessor(FMODWhitelist fmodWhitelist) : IClientPacketProcessor<FMODAssetPacket>
{
    private readonly FMODWhitelist fmodWhitelist = fmodWhitelist;

    public Task Process(ClientProcessorContext context, FMODAssetPacket packet)
    {
        if (!fmodWhitelist.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            Log.ErrorOnce($"[{nameof(FMODAssetProcessor)}] Whitelist has no item for {packet.AssetPath}.");
            return Task.CompletedTask;
        }

        FMODEmitterController.PlayEventOneShot(packet.AssetPath, soundData.Radius, packet.Position.ToUnity(), packet.Volume);
        return Task.CompletedTask;
    }
}
