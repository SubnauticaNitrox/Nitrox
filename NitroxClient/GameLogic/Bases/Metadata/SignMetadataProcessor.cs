using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public class SignMetadataProcessor : GenericBasePieceMetadataProcessor<SignMetadata>
    {
        public override void UpdateMetadata(NitroxId id, SignMetadata metadata, bool initialSync)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(id);
            uGUI_SignInput sign = gameObject.GetComponentInChildren<uGUI_SignInput>(true);
            if (sign.AliveOrNull() != null)
            {
                sign.text = metadata.Text;
                sign.colorIndex = metadata.ColorIndex;
                sign.elementsState = metadata.Elements;
                sign.scaleIndex = metadata.ScaleIndex;
                sign.SetBackground(metadata.Background);
            }
            else
            {
                Log.Error($"SignMetaData Processing failed for {gameObject.name}({gameObject.transform.position}). No sign component found on object.");
            }
        }
    }
}
