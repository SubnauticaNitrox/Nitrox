using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SubNameInputMetadataExtractor : NamedColoredMetadataExtractor<SubNameInput, SubNameInputMetadata>
{
    public override SubNameInputMetadata Extract(SubNameInput subNameInput)
    {
        SubName subName = subNameInput.target;
        return new(subNameInput.selectedColorIndex, GetName(subName), GetColors(subName));
    }
}
