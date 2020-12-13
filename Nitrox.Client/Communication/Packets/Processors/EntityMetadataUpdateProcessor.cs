using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.Spawning.Metadata;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
