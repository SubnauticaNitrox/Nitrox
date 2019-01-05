using System;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public class SignMetadataProcessor : GenericBasePieceMetadataProcessor<SignMetadata>
    {
        public override void UpdateMetadata(string guid, SignMetadata metadata)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(guid);
            uGUI_SignInput sign = gameObject.GetComponentInChildren<uGUI_SignInput>();

            sign.text = metadata.Text;
            sign.colorIndex = metadata.ColorIndex;
            sign.elementsState = metadata.Elements;
            sign.scaleIndex = metadata.ScaleIndex;
            sign.SetBackground(metadata.Background);
        }
    }
}
