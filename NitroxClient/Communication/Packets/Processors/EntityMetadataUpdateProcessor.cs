using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class EntityMetadataUpdateProcessor : ClientPacketProcessor<EntityMetadataUpdate>
    {
        public override void Process(EntityMetadataUpdate update)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(update.Id);

            Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(update.NewValue);
            Validate.IsTrue(metadataProcessor.HasValue, $"No processor found for EntityMetadata of type {update.NewValue.GetType()}");
            
            metadataProcessor.Value.ProcessMetadata(gameObject, update.NewValue);
        }
    }
}
