using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class SeamothMetadataProcessor : GenericEntityMetadataProcessor<SeamothMetadata>
{
    private readonly IPacketSender packetSender;
    private readonly LiveMixinManager liveMixinManager;

    public SeamothMetadataProcessor(IPacketSender packetSender, LiveMixinManager liveMixinManager)
    {
        this.packetSender = packetSender;
        this.liveMixinManager = liveMixinManager;
    }

    public override void ProcessMetadata(GameObject gameObject, SeamothMetadata metadata)
    {
        SeaMoth seamoth = gameObject.GetComponent<SeaMoth>();
        if (!seamoth)
        {
            Log.Error($"Could not find seamoth on {gameObject.name}");
        }

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetLights(seamoth, metadata.LightsOn);
            SetHealth(seamoth, metadata.Health);
        }
    }

    private void SetLights(SeaMoth seamoth, bool lightsOn)
    {
        using (FMODSystem.SuppressSounds())
        {
            seamoth.toggleLights.SetLightsActive(lightsOn);
        }
    }

    private void SetHealth(SeaMoth seamoth, float health)
    {
        LiveMixin liveMixin = seamoth.RequireComponentInChildren<LiveMixin>(true);
        liveMixinManager.SyncRemoteHealth(liveMixin, health);
    }
}
