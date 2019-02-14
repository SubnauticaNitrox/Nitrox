using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning
{
    public class SubnauticaEntitySpawnPointFactory : EntitySpawnPointFactory
    {
        public override List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, Transform transform, GameObject gameObject)
        {
            List<EntitySpawnPoint> spawnPoints = new List<EntitySpawnPoint>();
            EntitySlotsPlaceholder entitySlotsPlaceholder = gameObject.GetComponent<EntitySlotsPlaceholder>();

            if (!ReferenceEquals(entitySlotsPlaceholder, null))
            {
                foreach (EntitySlotData entitySlotData in entitySlotsPlaceholder.slotsData)
                {
                    List<EntitySlot.Type> slotTypes = SlotsHelper.GetEntitySlotTypes(entitySlotData);
                    List<string> stringSlotTypes = slotTypes.Select(s => s.ToString()).ToList();

                    spawnPoints.Add(
                        new EntitySpawnPoint(absoluteEntityCell,
                                             entitySlotData.localPosition,
                                             entitySlotData.localRotation,
                                             stringSlotTypes,
                                             entitySlotData.density,
                                             entitySlotData.biomeType.ToString())
                    );
                }
            }
            else
            {
                spawnPoints.Add(
                    new EntitySpawnPoint(absoluteEntityCell, transform.Position, transform.Rotation, transform.Scale, gameObject.ClassId)
                );
            }

            return spawnPoints;
        }
    }
}
