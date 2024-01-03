using System.Linq;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class SubNameInputMetadataProcessor : EntityMetadataProcessor<SubNameInputMetadata>
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

        // Ensure the SubNameInput's object is active so that it receives events from its SubName
        gameObject.SetActive(true);

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            // Name and color applying must be applied before SelectedColorIndex
            SetNameAndColors(subName, metadata.Name, metadata.Colors);
            subNameInput.SetSelected(metadata.SelectedColorIndex);
        }
    }

    public static void SetNameAndColors(SubName subName, string text, NitroxVector3[] nitroxColors)
    {
        if (!string.IsNullOrEmpty(text))
        {
            subName.DeserializeName(text);
        }
        if (nitroxColors != null)
        {
            Vector3[] colors = nitroxColors.Select(c => c.ToUnity()).ToArray();
            subName.DeserializeColors(colors);
        }
    }
}
