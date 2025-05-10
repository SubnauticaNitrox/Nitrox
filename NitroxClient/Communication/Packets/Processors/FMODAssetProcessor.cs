using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODAssetProcessor : IClientPacketProcessor<FMODAssetPacket>
{
    private readonly FmodWhitelist fmodWhitelist;

    public FMODAssetProcessor(FmodWhitelist fmodWhitelist)
    {
        this.fmodWhitelist = fmodWhitelist;
    }

    public Task Process(IPacketProcessContext context, FMODAssetPacket packet)
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
