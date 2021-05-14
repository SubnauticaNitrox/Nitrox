using FMOD.Studio;
using FMODUnity;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
#pragma warning disable 618

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODAssetProcessor : ClientPacketProcessor<PlayFMODAsset>
    {
        public override void Process(PlayFMODAsset packet)
        {
            EventInstance instance = FMODUWE.GetEvent(packet.AssetPath);
            instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, packet.Radius);
            instance.setVolume(packet.Volume);
            instance.set3DAttributes(packet.Position.ToUnity().To3DAttributes());
            instance.start();
            instance.release();
        }
    }
}
