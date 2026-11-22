using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class PictureFrameMetadataProcessor : EntityMetadataProcessor<PictureFrameMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PictureFrameMetadata metadata)
    {
        if (gameObject.TryGetComponent(out PictureFrame pictureFrame))
        {
            using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
            {
                pictureFrame.SelectImage(metadata.ContentHash);
            }
        }
    }
}
