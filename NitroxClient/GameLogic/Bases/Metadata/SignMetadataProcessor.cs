using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public class SignMetadataProcessor : GenericBasePieceMetadataProcessor<SignMetadata>
    {
        public override void UpdateMetadata(NitroxId id, SignMetadata metadata)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(id);

            sign.text = metadata.Text;
            sign.colorIndex = metadata.ColorIndex;
            sign.elementsState = metadata.Elements;
            sign.scaleIndex = metadata.ScaleIndex;
            sign.SetBackground(metadata.Background);
            uGUI_SignInput sign = gameObject.GetComponentInChildren<uGUI_SignInput>(true);
        }
    }
}
