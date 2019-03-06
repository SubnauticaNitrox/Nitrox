using System.Collections;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning
{
    public class NoneEntitySpawner : IEntitySpawner
    {
        /**
         * Crash fish are spawned by the CrashHome in the Monobehaviours Start method
         */
        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            TechType techType = entity.TechType.Enum();
            GameObject prefab;

            if (!PrefabDatabase.TryGetPrefab(entity.ClassId, out prefab))
            {
                prefab = CraftData.GetPrefabForTechType(techType, false);
                if (prefab == null)
                {
                    return Optional<GameObject>.Of(Utils.CreateGenericLoot(techType));
                }
            }

            GameObject gameObject = Utils.SpawnFromPrefab(prefab, null);

            gameObject.transform.position = entity.Position;
            gameObject.transform.localScale = entity.LocalScale;

            if (parent.IsPresent() && parent.Get().name == "CellRoot(Clone)")
            {
                LargeWorldEntity ent = gameObject.GetComponent<LargeWorldEntity>();
                ent.transform.SetParent(parent.Get().transform, true);
                ent.OnAddToCell();
            }
            else if (gameObject.name == "CellRoot(Clone)")
            {
                BatchCells cells = BatchCells.GetFromPool(LargeWorldStreamer.main.cellManager, LargeWorldStreamer.main, new Int3(entity.AbsoluteEntityCell.BatchId.X, entity.AbsoluteEntityCell.BatchId.Y, entity.AbsoluteEntityCell.BatchId.Z)); // Grab an already available BatchCell
                EntityCell cell = cells.Add(new Int3(entity.AbsoluteEntityCell.CellId.X, entity.AbsoluteEntityCell.CellId.Y, entity.AbsoluteEntityCell.CellId.Z), entity.Level);
                cell.Initialize(); // Initialize our cell
                gameObject.GetComponent<LargeWorldEntityCell>().cell = cell;
                gameObject.transform.SetParent(LargeWorldStreamer.main.cellsRoot);
            }
            else if (parent.IsPresent())
            {
                gameObject.transform.SetParent(parent.Get().transform);
            }

            gameObject.transform.localPosition = entity.LocalPosition;
            gameObject.transform.localRotation = entity.LocalRotation;

            GuidHelper.SetNewGuid(gameObject, entity.Guid);
            gameObject.SetActive(true);

            CrafterLogic.NotifyCraftEnd(gameObject, entity.TechType.Enum());

            return Optional<GameObject>.Of(gameObject);
        }

        public bool SpawnsOwnChildren()
        {
            return false;
        }
    }
}
