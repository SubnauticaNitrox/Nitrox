using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using UWE;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal class SubnauticaUweWorldEntityFactory(WorldEntitiesResource resource) : IUweWorldEntityFactory
{
    private readonly WorldEntitiesResource resource = resource;

    public bool TryFind(string classId, out UweWorldEntity? uweWorldEntity)
    {
        if (resource.WorldEntitiesByClassId.TryGetValue(classId, out WorldEntityInfo worldEntityInfo))
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
