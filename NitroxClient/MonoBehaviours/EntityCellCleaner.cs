using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    /**
     * 
     * When an entity cell initially loads, we want to clean it of all of the UWE 
     * spawned assets.  We still allow it to be initialized and have its CellRoot
     * (liveRoot) just want to remove all of the entities.  This is done as such
     * because UWE uses deserializeObjectTree which also spawns the root (the server
     * does not authoritatively spawn this as of right now)
     * 
     */
    public class EntityCellCleaner : MonoBehaviour
    {
        public static EntityCellCleaner Main { get
            {
                if(main == null)
                {
                    main = new GameObject().AddComponent<EntityCellCleaner>();
                }

                return main;
            }
        }

        private static EntityCellCleaner main;

        private HashSet<EntityCell> previouslySeen = new HashSet<EntityCell>();
        private List<EntityCell> pendingCleaning = new List<EntityCell>();

        void Update()
        {
            if(Multiplayer.Main == null || !Multiplayer.Main.IsMultiplayer())
            {
                return;
            }

            // iterating backwards so we can simply 
            // TODO: as a perforance optimization, we can do this ever X seconds
            for (int i = pendingCleaning.Count - 1; i >= 0; i--)
            {
                EntityCell entityCell = pendingCleaning[i];

                if (entityCell.liveRoot != null)
                {
                    pendingCleaning.RemoveAt(i);
                    CleanCell(entityCell);
                }
            }
        }

        private void CleanCell(EntityCell entityCell)
        {
            foreach (Transform child in entityCell.liveRoot.transform)
            {
                if (child.gameObject.GetComponent<NitroxEntity>() == null)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        public void Add(EntityCell entityCell)
        {
            if(previouslySeen.Contains(entityCell))
            {
                return;
            }

            previouslySeen.Add(entityCell);
            pendingCleaning.Add(entityCell);
        }
    }
}
