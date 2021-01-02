using System;
using NitroxModel.Logger;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;


namespace NitroxClient.GameLogic.Containers
{
    /// <summary>
    /// Restore projected Plant growth based on age of item in Planter
    /// </summary>
    class PlantableContainerAddItemPostProcessor : ContainerAddItemPostProcessor
    {
        public override Type[] ApplicableComponents { get; } = { typeof(Plantable) };

        public override void process(GameObject item, ItemData itemData)
        {
            PlantableItemData plantableData = itemData as PlantableItemData;
            if (plantableData == null)
            {
                // nothing to do; false alarm
                return;
            }
            Plantable plant = item.GetComponent<Plantable>();
            if (plant == null)
            {
                Log.Error($"FixPlantGrowth: Item for Plantable {plantableData.ItemId} is not a Plantable!");
                return;
            }

            GrowingPlant grower = GetGrowingPlant(plant);
            if (grower == null)
            {
                Log.Error($"FixPlantGrowth: Could not find GrowingPlant for Plantable {plantableData.ItemId}!");
                return;
            }


            // time in seconds
            double elapsedGrowthTime = (DayNightCycle.main.timePassedAsDouble - plantableData.PlantedGameTime);

            if (elapsedGrowthTime > grower.growthDuration)
            {
                // should be ready
                Log.Debug($"FixPlantGrowth: Finishing {item.name} {plantableData.ItemId} that has grown for {elapsedGrowthTime} seconds");
                grower.SetProgress(1.0f);
            }
            else
            {
                Log.Debug($"FixPlantGrowth: Growing {item.name} {plantableData.ItemId} that has grown for {elapsedGrowthTime} seconds");
                grower.SetProgress(Convert.ToSingle(elapsedGrowthTime / grower.growthDuration));
            }
        }

        public static GrowingPlant GetGrowingPlant(Plantable plantable)
        {
            int slot = plantable.GetSlotID();

            Planter pp = plantable.currentPlanter;
            if (null == pp)
            {
                Log.Error($"GetGrowingPlant: plant not inside a Planter!");
                return null;
            }

            // int smallSlotCount = pp.slots.Length;
            int bigSlotCount = pp.bigSlots.Length;

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
