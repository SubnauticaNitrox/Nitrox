using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODAssetProcessor : ClientPacketProcessor<FMODAssetPacket>
{
    private readonly FMODWhitelist fmodWhitelist;

    public FMODAssetProcessor(FMODWhitelist fmodWhitelist)
    {
        this.fmodWhitelist = fmodWhitelist;
    }

    public override void Process(FMODAssetPacket packet)
    {
        if (!fmodWhitelist.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            Log.ErrorOnce($"[{nameof(FMODAssetProcessor)}] Whitelist has no item for {packet.AssetPath}.");
            return;
        }

        FMODEmitterController.PlayEventOneShot(packet.AssetPath, soundData.Radius, packet.Position.ToUnity(), packet.Volume);
    }
}
