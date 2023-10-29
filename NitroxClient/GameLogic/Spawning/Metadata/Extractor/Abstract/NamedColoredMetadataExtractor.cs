using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

public abstract class NamedColoredMetadataExtractor<I, O> : GenericEntityMetadataExtractor<I, O> where O : NamedColoredMetadata
{
    public string GetName(SubName subName)
    {
        return subName.AliveOrNull()?.hullName.AliveOrNull()?.text;
    }

    public NitroxVector3[] GetColors(SubName subName)
    {
        return subName.AliveOrNull()?.GetColors().Select(color => color.ToDto()).ToArray();
    }
}
