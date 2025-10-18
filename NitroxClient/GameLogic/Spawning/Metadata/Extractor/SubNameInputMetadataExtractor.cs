using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SubNameInputMetadataExtractor : EntityMetadataExtractor<SubNameInput, SubNameInputMetadata>
{
    public override SubNameInputMetadata Extract(SubNameInput subNameInput)
    {
        SubName subName = subNameInput.target;
        return new(subNameInput.selectedColorIndex, GetName(subName), GetColors(subName));
    }

    public static string GetName(SubName subName)
    {
        return subName.AliveOrNull()?.hullName.AliveOrNull()?.text;
    }

    public static NitroxVector3[] GetColors(SubName subName)
    {
        return subName.AliveOrNull()?.GetColors().Select(color => color.ToDto()).ToArray();
    }
}
