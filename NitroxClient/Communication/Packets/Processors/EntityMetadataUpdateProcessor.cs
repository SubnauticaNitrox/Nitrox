using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Networking.Packets;

using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityMetadataUpdateProcessor : IClientPacketProcessor<EntityMetadataUpdate>
{
    private readonly EntityMetadataManager entityMetadataManager;

    public EntityMetadataUpdateProcessor(EntityMetadataManager entityMetadataManager)
    {
        this.entityMetadataManager = entityMetadataManager;
    }

    public Task Process(IPacketProcessContext context, EntityMetadataUpdate update)
    {
        if (!NitroxEntity.TryGetObjectFrom(update.Id, out GameObject gameObject))
        {
            entityMetadataManager.RegisterNewerMetadata(update.Id, update.NewValue);
            return Task.CompletedTask;
        }

        Optional<IEntityMetadataProcessor> metadataProcessor = entityMetadataManager.FromMetaData(update.NewValue);
        Validate.IsTrue(metadataProcessor.HasValue, $"No processor found for EntityMetadata of type {update.NewValue.GetType()}");

        metadataProcessor.Value.ProcessMetadata(gameObject, update.NewValue);
        return Task.CompletedTask;
    }
}
