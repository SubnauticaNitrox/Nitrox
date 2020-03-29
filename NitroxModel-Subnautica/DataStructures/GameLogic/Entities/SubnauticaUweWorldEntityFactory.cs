using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Helper;
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
            WorldEntityInfo worldEntityInfo;

            if (worldEntitiesByClassId.TryGetValue(classId, out worldEntityInfo))
            {
                UweWorldEntity uweWorldEntity = new UweWorldEntity(worldEntityInfo.techType.Model(),
                                                                   worldEntityInfo.localScale,
                                                                   worldEntityInfo.classId,
                                                                   worldEntityInfo.slotType.ToString(),
                                                                   (int)worldEntityInfo.cellLevel);

                return Optional.Of(uweWorldEntity);
            }

            return Optional.Empty;
        }
    }
}
