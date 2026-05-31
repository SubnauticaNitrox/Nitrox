using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class EntityMetadataUpdateProcessor(Entities entities, EntityMetadataManager entityMetadataManager) : IClientPacketProcessor<EntityMetadataUpdate>
{
    private readonly Entities entities = entities;
    private readonly EntityMetadataManager entityMetadataManager = entityMetadataManager;

    public Task Process(ClientProcessorContext context, EntityMetadataUpdate update)
    {
        if (entities.SpawningEntities)
        {
            entityMetadataManager.RegisterNewerMetadata(update.Id, update.NewValue);
        }

        if (!NitroxEntity.TryGetObjectFrom(update.Id, out GameObject gameObject))
        {
            return Task.CompletedTask;
        }

        Optional<IEntityMetadataProcessor> metadataProcessor = entityMetadataManager.FromMetaData(update.NewValue);
        Validate.IsTrue(metadataProcessor.HasValue, $"No processor found for EntityMetadata of type {update.NewValue.GetType()}");

        metadataProcessor.Value.ProcessMetadata(gameObject, update.NewValue);
        return Task.CompletedTask;
    }
}
