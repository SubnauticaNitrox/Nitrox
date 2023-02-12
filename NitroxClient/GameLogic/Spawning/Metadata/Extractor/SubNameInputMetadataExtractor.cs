using System.Linq;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SubNameInputMetadataExtractor : GenericEntityMetadataExtractor<SubNameInput, SubNameInputMetadata>
{
    public override SubNameInputMetadata Extract(SubNameInput subNameInput)
    {
        NitroxVector3[] colors = subNameInput.target.GetColors().Select(color => color.ToDto()).ToArray();

        return new(subNameInput.inputField.text, colors);
    }
}
