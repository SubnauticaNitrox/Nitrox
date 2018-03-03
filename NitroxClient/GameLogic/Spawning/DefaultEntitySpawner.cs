using NitroxModel.DataStructures.GameLogic;
using NitroxClient.GameLogic.Helper;
using UnityEngine;
using NitroxModel.DataStructures.Util;

namespace NitroxClient.GameLogic.Spawning
{
    public class DefaultEntitySpawner : IEntitySpawner
    {
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            GameObject gameObject = CraftData.InstantiateFromPrefab(entity.TechType);
            gameObject.transform.position = entity.Position;
            gameObject.transform.localRotation = entity.Rotation;
            GuidHelper.SetNewGuid(gameObject, entity.Guid);
            gameObject.SetActive(true);
            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, entity.TechType);
                
            return Optional<GameObject>.Of(gameObject);
        }
    }
}
