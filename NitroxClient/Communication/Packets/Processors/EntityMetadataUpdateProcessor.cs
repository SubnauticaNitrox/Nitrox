using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityMetadataUpdateProcessor : ClientPacketProcessor<EntityMetadataUpdate>
{
    private readonly EntityMetadataManager entityMetadataManager;

    public EntityMetadataUpdateProcessor(EntityMetadataManager entityMetadataManager)
    {
        this.entityMetadataManager = entityMetadataManager;
    }

    public override void Process(EntityMetadataUpdate update)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(update.Id);

        Optional<IEntityMetadataProcessor> metadataProcessor = entityMetadataManager.FromMetaData(update.NewValue);
        Validate.IsTrue(metadataProcessor.HasValue, $"No processor found for EntityMetadata of type {update.NewValue.GetType()}");

        metadataProcessor.Value.ProcessMetadata(gameObject, update.NewValue);
    }
}
