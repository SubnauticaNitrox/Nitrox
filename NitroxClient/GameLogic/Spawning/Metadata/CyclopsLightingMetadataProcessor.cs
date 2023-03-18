using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class CyclopsLightingMetadataProcessor : GenericEntityMetadataProcessor<CyclopsLightingMetadata>
{
    private readonly IPacketSender packetSender;

    public CyclopsLightingMetadataProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public override void ProcessMetadata(GameObject gameObject, CyclopsLightingMetadata metadata)
    {
        CyclopsLightingPanel lighting = gameObject.RequireComponentInChildren<CyclopsLightingPanel>(true);

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetInternalLighting(lighting, metadata.InternalLightsOn);
            SetFloodLighting(lighting, metadata.FloodLightsOn);
        }
    }

    private void SetInternalLighting(CyclopsLightingPanel lighting, bool isOn)
    {
        if (lighting.lightingOn == isOn)
        {
            return;
        }

        lighting.lightingOn = !lighting.lightingOn;
        lighting.cyclopsRoot.ForceLightingState(lighting.lightingOn);
        FMODAsset asset = (!lighting.lightingOn) ? lighting.vn_lightsOff : lighting.vn_lightsOn;
        FMODUWE.PlayOneShot(asset, lighting.transform.position, 1f);
        lighting.UpdateLightingButtons();
    }

    private void SetFloodLighting(CyclopsLightingPanel lighting, bool isOn)
    {
        if (lighting.floodlightsOn == isOn)
        {
            return;
        }

        lighting.floodlightsOn = !lighting.floodlightsOn;
        lighting.SetExternalLighting(lighting.floodlightsOn);
        FMODAsset asset = !lighting.floodlightsOn ? lighting.vn_floodlightsOff : lighting.vn_floodlightsOn;
        FMODUWE.PlayOneShot(asset, lighting.transform.position, 1f);
        lighting.UpdateLightingButtons();
    }
}
