using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal static class ScannerResourceDescriptorFactory
{
    public static bool TryCreate(bool enabled, int trackerTechType, int overrideTechType, int prefabTechType, ushort trackerIndex, NitroxVector3 relativePosition, out ScannerResourceDescriptor descriptor)
    {
        descriptor = default;
        if (!enabled)
        {
            return false;
        }

        int effectiveTechType = overrideTechType != (int)TechType.None
                                    ? overrideTechType
                                    : trackerTechType != (int)TechType.None
                                        ? trackerTechType
                                        : prefabTechType;

        if (effectiveTechType == (int)TechType.None || !Enum.IsDefined(typeof(TechType), effectiveTechType))
        {
            return false;
        }

        descriptor = new ScannerResourceDescriptor(new NitroxTechType(((TechType)effectiveTechType).ToString()), trackerIndex, relativePosition);
        return true;
    }
}
