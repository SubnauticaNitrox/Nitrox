using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class SubNameInputProcessor : GenericEntityMetadataProcessor<SubNameInputMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, SubNameInputMetadata metadata)
    {        
        SubNameInput subNameInput = gameObject.GetComponent<SubNameInput>();

        if (subNameInput)
        {
            subNameInput.SetName(metadata.Name);
            subNameInput.target.SetName(subNameInput.name);

            for (int i = 0; i < metadata.Colors.Length; i++)
            {
                Vector3 hsb = metadata.Colors[i].ToUnity();
                Color color = uGUI_ColorPicker.HSBToColor(hsb);
                subNameInput.SetColor(i, color);
                subNameInput.target.SetColor(i, hsb, color);
            }

            subNameInput.SetSelected(0);
        }
        else
        {
            Log.Error($"Could not find SubNameInput on {gameObject.name}");
        }
    }
}
