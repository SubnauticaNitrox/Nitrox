using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class SubNameInputMetadataProcessor : NamedColoredMetadataProcessor<SubNameInputMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, SubNameInputMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out SubNameInput subNameInput))
        {
            Log.ErrorOnce($"[{nameof(SubNameInputMetadataProcessor)}] Could not find {nameof(SubNameInput)} on {gameObject}");
            return;
        }

        SubName subName = subNameInput.target;
        if (!subName && !subNameInput.TryGetComponent(out subName))
        {
            Log.ErrorOnce($"[{nameof(SubNameInputMetadataProcessor)}] {gameObject}'s {nameof(subNameInput)} doesn't have a target.");
            return;
        }

        // Name and color applying must be applied before SelectedColorIndex
        base.ProcessMetadata(subName.gameObject, metadata);
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            subNameInput.SetSelected(metadata.SelectedColorIndex);
        }
    }
}
