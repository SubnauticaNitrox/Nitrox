using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UWE;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Entities
{
    public class SubnauticaUweWorldEntityFactory : UweWorldEntityFactory
    {
        private Dictionary<string, WorldEntityInfo> worldEntitiesByClassId;

        public SubnauticaUweWorldEntityFactory(Dictionary<string, WorldEntityInfo> worldEntitiesByClassId)
        {
            this.worldEntitiesByClassId = worldEntitiesByClassId;
        }

        public override Optional<UweWorldEntity> From(string classId)
        {

            if (worldEntitiesByClassId.TryGetValue(classId, out WorldEntityInfo worldEntityInfo))
            {
                UweWorldEntity uweWorldEntity = new UweWorldEntity(worldEntityInfo.techType.ToDto(),
                                                                   worldEntityInfo.localScale.ToDto(),
                                                                   worldEntityInfo.classId,
                                                                   worldEntityInfo.slotType.ToString(),
                                                                   (int)worldEntityInfo.cellLevel);

                return Optional.Of(uweWorldEntity);
            }

            return Optional.Empty;
        }
    }
}
