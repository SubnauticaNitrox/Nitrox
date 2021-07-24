using FMOD.Studio;
using FMODUnity;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

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
            // Volume is a scalar, is should be limited to 0 and we don't need more than 100% volume (i.e. 1.0).
            // See docs: https://fmod.com/resources/documentation-api?version=2.00&page=studio-api-eventinstance.html#studio_eventinstance_setvolume
            instance.setVolume(Mathf.Clamp01(packet.Volume));
            instance.set3DAttributes(packet.Position.ToUnity().To3DAttributes());
            instance.start();
            instance.release();
        }
    }
}
