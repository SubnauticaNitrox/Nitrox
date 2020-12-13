using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Metadata;
using UnityEngine;

namespace Nitrox.Client.GameLogic.Bases.Metadata
{
    public class SignMetadataProcessor : GenericBasePieceMetadataProcessor<SignMetadata>
    {
        public override void UpdateMetadata(NitroxId id, SignMetadata metadata)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(id);
            uGUI_SignInput sign = gameObject.GetComponentInChildren<uGUI_SignInput>();

            sign.text = metadata.Text;
            sign.colorIndex = metadata.ColorIndex;
            sign.elementsState = metadata.Elements;
            sign.scaleIndex = metadata.ScaleIndex;
            sign.SetBackground(metadata.Background);
        }
    }
}
