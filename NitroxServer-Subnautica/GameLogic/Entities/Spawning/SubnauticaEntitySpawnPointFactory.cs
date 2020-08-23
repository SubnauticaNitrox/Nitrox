﻿using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxServer.UnityStubs;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning
{
    public class SubnauticaEntitySpawnPointFactory : EntitySpawnPointFactory
    {
        private readonly Dictionary<string, EntitySpawnPoint> spawnPointsByUid = new Dictionary<string, EntitySpawnPoint>();

        public override List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, TransformAsset transform, GameObject gameObject)
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
                                             entitySlotData.localPosition.ToDto(),
                                             entitySlotData.localRotation.ToDto(),
                                             stringSlotTypes,
                                             entitySlotData.density,
                                             entitySlotData.biomeType.ToString());


                    HandleParenting(spawnPoints, entitySpawnPoint, gameObject);
                }
            }
            else
            {
                EntitySpawnPoint entitySpawnPoint = new EntitySpawnPoint(absoluteEntityCell, transform.LocalPosition, transform.LocalRotation, transform.LocalScale, gameObject.ClassId);

                HandleParenting(spawnPoints, entitySpawnPoint, gameObject);
            }

            return spawnPoints;
        }

        private void HandleParenting(List<EntitySpawnPoint> spawnPoints, EntitySpawnPoint entitySpawnPoint, GameObject gameObject)
        {
            EntitySpawnPoint parent;
            if (gameObject.Parent != null && spawnPointsByUid.TryGetValue(gameObject.Parent, out parent))
            {
                entitySpawnPoint.Parent = parent;
                parent.Children.Add(entitySpawnPoint);
            }

            spawnPointsByUid[gameObject.Id] = entitySpawnPoint;

            if (gameObject.Parent == null)
            {
                spawnPoints.Add(
                    entitySpawnPoint
                );
            }
        }
    }
}
