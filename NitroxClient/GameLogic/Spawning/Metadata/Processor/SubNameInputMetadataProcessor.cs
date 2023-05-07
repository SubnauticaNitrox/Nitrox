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
#if SUBNAUTICA
        SubName subName = subNameInput.target;
        if (!subName && !subNameInput.TryGetComponent(out subName))
#elif BELOWZERO
        ICustomizeable subName = subNameInput.target;
        if (subName == null && !subNameInput.TryGetComponent(out subName))
#endif
        {
            Log.ErrorOnce($"[{nameof(SubNameInputMetadataProcessor)}] {gameObject}'s {nameof(subNameInput)} doesn't have a target.");
            return;
        }

        // Ensure the SubNameInput's object is active so that it receives events from its SubName
        gameObject.SetActive(true);

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            // Name and color applying must be applied before SelectedColorIndex
#if SUBNAUTICA
            SetNameAndColors(subName, metadata.Name, metadata.Colors);
#elif BELOWZERO
            SetNameAndColors(subNameInput, subName, metadata.Name, metadata.Colors);
#endif
            subNameInput.SetSelected(metadata.SelectedColorIndex);
        }
    }

#if SUBNAUTICA
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
#elif BELOWZERO
    public static void SetNameAndColors(SubNameInput subNameInput, ICustomizeable iCustomizeable, string text, NitroxVector3[] nitroxColors)
    {
        if (!string.IsNullOrEmpty(text))
        {
            subNameInput.SetName(iCustomizeable.GetName());
        }
        if (nitroxColors != null)
        {
            Vector3[] colors = nitroxColors.Select(c => c.ToUnity()).ToArray();
            subNameInput.DeserialiseColors(colors);
        }
    }
#endif
}
