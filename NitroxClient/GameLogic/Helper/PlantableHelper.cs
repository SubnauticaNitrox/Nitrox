using System;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.GameLogic.Helper
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Fix Plant growth progress based on delivewred timestamps.
        /// </summary>
        /// <param name="gameObject">The plant in question</param>
        /// <param name="plantableData">Timing data delivered from the server</param>
        public static void FixPlantGrowth(this GameObject gameObject, PlantableItemData plantableData)
        {
            Plantable plant = gameObject.GetComponent<Plantable>();
            if (null == plant)
            {
                Log.Error($"FixPlantGrowth: Item for Plantable {plantableData.ItemId} is not a Plantable!");
                return;
            }

            GrowingPlant grower = plant.GetGrowingPlant();
            if (null == grower)
            {
                Log.Error($"FixPlantGrowth: Could not find GrowingPlant for Plantable {plantableData.ItemId}!");
                return;
            }


            // time in seconds
            double elapsedGrowthTime = (DayNightCycle.main.timePassedAsDouble - plantableData.PlantedGameTime);

            if (elapsedGrowthTime > grower.growthDuration)
            {
                // should be ready
                Log.Debug($"FixPlantGrowth: Finishing {gameObject.name} {plantableData.ItemId} that has grown for {elapsedGrowthTime} seconds");
                grower.SetProgress(1.0f);
            }
            else
            {
                Log.Debug($"FixPlantGrowth: Growing {gameObject.name} {plantableData.ItemId} that has grown for {elapsedGrowthTime} seconds");
                grower.SetProgress(Convert.ToSingle(elapsedGrowthTime / grower.growthDuration));
            }
        }

        /// <summary>
        /// Pereform a recursive dump of the objectr into the clients log file.
        /// The dump will contain a parent trace (object names and types)
        /// and a tree of all children (defined by Transform).
        /// For each object shown, the attached Componet types are listed.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="currentDepth"></param>
        /// <param name="maxDepth"></param>
        public static void DumpRecursive(this GameObject gameObject, int maxDepth = 10, int currentDepth=0)
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
                        else
                        {
                            GrowingPlant gpl = pt.GetGrowingPlant();
                            if (gpl)
                            {
                                components += $"(slot {pt.GetSlotID()} - {gpl.GetProgress()})";
                            }
                            else
                            {
                                components += $"(slot {pt.GetSlotID()} - ???)";
                            }
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

        /// <summary>
        /// Decisions based on object names are not a good idea. Anyway, this method will return a basic name without leading dollar signs or the "(Clone)" info.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static string GetNormalizedName(this GameObject gameObject)
        {
            return (
                gameObject.name[0] == '$'
                ? gameObject.name.Substring(1)
                : gameObject.name
            ).Split(" (".ToCharArray())[0];
        }
    }

    public static class PlantableExtension
    {
        /// <summary>
        /// Return the GrowingPlant object linked to the Plantable, if there is one.
        /// There can only be such a GrowingPlant if the Plantable has been inserted into a Planter
        /// </summary>
        /// <param name="plantable">The Plantable item</param>
        /// <returns></returns>
        public static GrowingPlant GetGrowingPlant(this Plantable plantable)
        {
            int slot = plantable.GetSlotID();

            Planter pp = plantable.currentPlanter;
            if( null==pp)
            {
                Log.Error($"GetGrowingPlant: plant not inside a Planter!");
                return null;
            }

            // int smallSlotCount = pp.slots.Length;
            int bigSlotCount = pp.bigSlots.Length;

            // Debug code in case things go south with some new planter type
            /*
                switch (plantable.currentPlanter.gameObject.GetNormalizedName())
                {
                    case "PlanterBox":
                    case "FarmingTray":
                    case "PlanterPot":
                    case "PlanterPot2":
                    case "PlanterPot3":
                    case "PlanterShelf":
                        break;
                    default:
                        Log.Error($"Unexpected Planter type {plantable.currentPlanter.gameObject.name} - slot {slot} of {smallSlotCount} + {bigSlotCount}");
                        break;
                }
            */
            // for all the planters I have seen, the logic is the same: Available slots are numbered starting with the big slots
            if (slot < bigSlotCount)
            {
                // index 0 .. #big-1
                return pp.bigSlots[slot].GetComponentInChildren<GrowingPlant>();
            }
            else
            {
                // index #big .. #big+#small-1
                return pp.slots[slot - bigSlotCount].GetComponentInChildren<GrowingPlant>();
            }
        }
    }
}
