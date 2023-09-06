using FMOD.Studio;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
#pragma warning disable CS0618

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
            Log.Error($"[FMODAssetProcessor] Whitelist has no item for {packet.AssetPath}.");
            return;
        }

        EventInstance instance = FMODUWE.GetEvent(packet.AssetPath);
        instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, soundData.Radius);
        instance.setVolume(packet.Volume);
        instance.set3DAttributes(packet.Position.To3DAttributes());
        instance.start();
        instance.release();
    }
}
