using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class EntitySignMetadataProcessor : EntityMetadataProcessor<EntitySignMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, EntitySignMetadata metadata)
    {
        uGUI_SignInput sign = gameObject.GetComponentInChildren<uGUI_SignInput>(true);
        if (sign)
        {
            sign.text = metadata.Text;
            sign.colorIndex = metadata.ColorIndex;
            sign.elementsState = metadata.Elements;
            sign.scaleIndex = metadata.ScaleIndex;
            sign.SetBackground(metadata.Background);

            // TMP_InputField.text (set above via sign.text) doesn't force its visible mesh to redraw
            // when set from script instead of by typing. ForceLabelUpdate() is TextMeshPro's own
            // escape hatch for exactly this "set from code, doesn't redraw" case.
            sign.inputField.ForceLabelUpdate();
        }
    }
}
