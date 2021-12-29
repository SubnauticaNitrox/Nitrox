using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class EntitySignMetadataProcessor : GenericEntityMetadataProcessor<EntitySignMetadata>
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
        }
    }
}
