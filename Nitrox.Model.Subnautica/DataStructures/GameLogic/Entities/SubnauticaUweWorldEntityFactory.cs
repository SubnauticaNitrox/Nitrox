using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic.Entities;
using UWE;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

public class SubnauticaUweWorldEntityFactory : IUweWorldEntityFactory
{
    private readonly Dictionary<string, WorldEntityInfo> worldEntitiesByClassId;

    public SubnauticaUweWorldEntityFactory(Dictionary<string, WorldEntityInfo> worldEntitiesByClassId)
    {
        this.worldEntitiesByClassId = worldEntitiesByClassId;
    }

    public bool TryFind(string classId, out UweWorldEntity uweWorldEntity)
    {
        if (worldEntitiesByClassId.TryGetValue(classId, out WorldEntityInfo worldEntityInfo))
        {
            uweWorldEntity = new(worldEntityInfo.classId,
                                 worldEntityInfo.techType.ToDto(),
                                 worldEntityInfo.slotType.ToString(),
                                 worldEntityInfo.prefabZUp,
                                 (int)worldEntityInfo.cellLevel,
                                 worldEntityInfo.localScale.ToDto());

            return true;
        }
        uweWorldEntity = null;
        return false;
    }
}
