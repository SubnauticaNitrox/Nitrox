using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning
{
    public class SubnauticaEntitySpawnPointFactory : EntitySpawnPointFactory
    {
        private readonly Dictionary<string, EntitySpawnPoint> spawnPointsByUid = new Dictionary<string, EntitySpawnPoint>();

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
                    EntitySpawnPoint entitySpawnPoint = new EntitySpawnPoint(absoluteEntityCell,
                                             entitySlotData.localPosition,
                                             entitySlotData.localRotation,
                                             stringSlotTypes,
                                             entitySlotData.density,
                                             entitySlotData.biomeType.ToString());


                    BuildChildren(entitySpawnPoint, gameObject);
                    if (gameObject.Parent == null)
                    {
                        spawnPoints.Add(
                            entitySpawnPoint
                        );
                    }
                }
            }
            else
            {
                EntitySpawnPoint entitySpawnPoint = new EntitySpawnPoint(absoluteEntityCell, transform.Position, transform.Rotation, transform.Scale, gameObject.ClassId);

                BuildChildren(entitySpawnPoint, gameObject);

                if (gameObject.Parent == null)
                {
                    spawnPoints.Add(
                        entitySpawnPoint
                    );
                }
            }

            return spawnPoints;
        }

        private void BuildChildren(EntitySpawnPoint entitySpawnPoint, GameObject gameObject)
        {
            EntitySpawnPoint parent;
            if (gameObject.Parent != null && spawnPointsByUid.TryGetValue(gameObject.Parent, out parent))
            {
                entitySpawnPoint.SetParent(parent);
                parent.Children.Add(entitySpawnPoint);
            }

            spawnPointsByUid[gameObject.Id] = entitySpawnPoint;
        }
    }
}
