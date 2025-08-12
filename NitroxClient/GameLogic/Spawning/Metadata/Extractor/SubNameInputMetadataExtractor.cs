//TODO: Rework the naming
#if SUBNAUTICA
using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SubNameInputMetadataExtractor : EntityMetadataExtractor<SubNameInput, SubNameInputMetadata>
{
    public override SubNameInputMetadata Extract(SubNameInput subNameInput)
    {
#if SUBNAUTICA
        SubName subName = subNameInput.target;
#elif BELOWZERO
        ICustomizeable subName = subNameInput.target;
#endif
        return new(subNameInput.selectedColorIndex, GetName(subName), GetColors(subName));
    }
#if SUBNAUTICA
    public static string GetName(SubName subName)
    {
        return subName.AliveOrNull()?.hullName.AliveOrNull()?.text;
    }

    public static NitroxVector3[] GetColors(SubName subName)
    {
        return subName.AliveOrNull()?.GetColors().Select(color => color.ToDto()).ToArray();
    }
#elif BELOWZERO
    public static string GetName(ICustomizeable iCustomizeable)
    {
        ColorNameControl colorNameControl = iCustomizeable as ColorNameControl;
        if (colorNameControl != null)
        {
            return colorNameControl.AliveOrNull()?.namePlate.AliveOrNull()?.namePlateText;
        }

        return iCustomizeable.GetName();
    }

    public static NitroxVector3[] GetColors(ICustomizeable iCustomizeable)
    {
        ColorNameControl colorNameControl = iCustomizeable as ColorNameControl;
        if (colorNameControl != null)
        {
            return colorNameControl.AliveOrNull()?.savedColors.Select(color => color.ToDto()).ToArray();
        }

        BaseName baseName = iCustomizeable as BaseName;
        if (baseName != null)
        {
            return baseName.AliveOrNull() ?.baseColors.Select(color => color.ToDto()).ToArray();
        }

        return iCustomizeable.GetColors().Select(color => color.ToDto()).ToArray();
    }
#endif
}
#endif
