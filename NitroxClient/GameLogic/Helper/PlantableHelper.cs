using UnityEngine;
using NitroxModel.Logger;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.GameLogic.Helper
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Pereform a recursive dump of the objectr into the clients log file.
        /// The dump will contain a parent trace (object names and types)
        /// and a tree of all children (defined by Transform).
        /// For each object shown, the attached Componet types are listed.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="currentDepth"></param>
        /// <param name="maxDepth"></param>
        public static void DumpRecursive(this GameObject gameObject, int maxDepth = 10, int currentDepth = 0)
        {
            if (currentDepth == 0)
            {
                // get all parents first
                if (gameObject.transform.parent)
                {
                    gameObject.transform.parent.gameObject.DumpRecursive(maxDepth, -1);
                }
            }
            if (currentDepth < 0)
            {
                // tail recursion
                if (gameObject.transform.parent)
                {
                    gameObject.transform.parent.gameObject.DumpRecursive(maxDepth, currentDepth - 1);
                }
                NitroxEntity entity = gameObject.GetComponent<NitroxEntity>();
                if (null != entity)
                {
                    Log.Info(new string('<', -currentDepth) + $" {gameObject.name} -- {gameObject.GetType()} -- {entity.Id}");
                }
                else
                {
                    Log.Info(new string('<', -currentDepth) + $" {gameObject.name} -- {gameObject.GetType()} -- no entity");
                }
            }
            else
            {
                // print current object/Component
                Log.Info(new string('>', currentDepth) + $" {gameObject.name} -- {gameObject.GetType()}");
                NitroxEntity ent = gameObject.GetComponent<NitroxEntity>();
                if (ent)
                {
                    Log.Info(new string(' ', currentDepth + 3) + $"-- {ent.Id}");
                }

                // show all components
                string components = new string(' ', currentDepth + 3) + "Components: ";
                foreach (Component c in gameObject.GetComponents<Component>())
                {
                    components += ", " + c.GetType();
                    if (c is GrowingPlant gp)
                    {
                        components += $" ({gp.GetProgress()} of {gp.growthDuration})";
                    }
                    if (c is Plantable pt && pt.currentPlanter)
                    {
                        if (pt.linkedGrownPlant)
                        {
                            components += $" (slot {pt.GetSlotID()} - GROWN)";
                        }
                    }
                }
                Log.Info(components);

                // recurse unless
                if (currentDepth >= maxDepth)
                {
                    return;
                }

                // recurse geometry
                foreach (Transform child in gameObject.transform)
                {
                    child.gameObject.DumpRecursive(maxDepth, currentDepth + 1);
                }
            }
        }
    }
}
