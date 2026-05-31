using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using UWE;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal class SubnauticaUweWorldEntityFactory(WorldEntitiesResource resource)
{
    private readonly WorldEntitiesResource resource = resource;

    public async Task<UweWorldEntity?> FindAsync(string classId)
    {
        Dictionary<string, WorldEntityInfo> worldEntitiesByClassId = await resource.GetWorldEntitiesByClassIdAsync();
        if (worldEntitiesByClassId.TryGetValue(classId, out WorldEntityInfo worldEntityInfo))
        {
            return new(worldEntityInfo.classId,
                                 worldEntityInfo.techType.ToDto(),
                                 worldEntityInfo.slotType.ToString(),
                                 worldEntityInfo.prefabZUp,
                                 (int)worldEntityInfo.cellLevel,
                                 worldEntityInfo.localScale.ToDto());
        }
        return null;
    }
}
