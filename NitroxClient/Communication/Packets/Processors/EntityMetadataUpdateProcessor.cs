using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
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
    private readonly Entities entities;
    private readonly EntityMetadataManager entityMetadataManager;

    public EntityMetadataUpdateProcessor(Entities entities, EntityMetadataManager entityMetadataManager)
    {
        this.entities = entities;
        this.entityMetadataManager = entityMetadataManager;
    }

    public override void Process(EntityMetadataUpdate update)
    {
        if (entities.SpawningEntities)
        {
            entityMetadataManager.RegisterNewerMetadata(update.Id, update.NewValue);
        }

        if (!NitroxEntity.TryGetObjectFrom(update.Id, out GameObject gameObject))
        {
            return;
        }

        Optional<IEntityMetadataProcessor> metadataProcessor = entityMetadataManager.FromMetaData(update.NewValue);
        Validate.IsTrue(metadataProcessor.HasValue, $"No processor found for EntityMetadata of type {update.NewValue.GetType()}");

        metadataProcessor.Value.ProcessMetadata(gameObject, update.NewValue);
    }
}
