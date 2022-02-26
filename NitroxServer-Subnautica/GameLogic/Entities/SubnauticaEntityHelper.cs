using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities;
using NitroxServer_Subnautica.Serialization.Resources;

namespace NitroxServer_Subnautica.GameLogic.Entities;

public class SubnauticaEntityHelper : EntityHelper
{
    public override bool TryGetTechTypeForClassId(string classId, out NitroxTechType nitroxTechType)
    {
        ResourceAssets resourceAssets = ResourceAssetsParser.Parse();
        if (resourceAssets.WorldEntitiesByClassId.TryGetValue(classId, out UWE.WorldEntityInfo worldEntityInfo))
        {
            nitroxTechType = worldEntityInfo.techType.ToDto();
            // At this point, the tech type can still be None, so we need to make sure we return true only if we have a valid tech type
            return nitroxTechType.ToUnity() != TechType.None;
        }
        nitroxTechType = default;
        return false;
    }
}
