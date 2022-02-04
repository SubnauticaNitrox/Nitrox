using System;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;


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
            if (itemData is not PlantableItemData plantableData)
            {
                // nothing to do; false alarm
                return;
            }
            Plantable plant = item.GetComponent<Plantable>();
            if (!plant)
            {
                Log.Error($"FixPlantGrowth: Item for Plantable {plantableData.ItemId} is not a Plantable!");
                return;
            }

            GrowingPlant grower = GetGrowingPlant(plant);
            if (!grower)
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

        private static GrowingPlant GetGrowingPlant(Plantable plantable)
        {
            int slot = plantable.GetSlotID();

            Planter planter = plantable.currentPlanter;
            if (!planter)
            {
                Log.Error($"GetGrowingPlant: plant not inside a Planter!");
                return null;
            }

            // int smallSlotCount = pp.slots.Length;
            int bigSlotCount = planter.bigSlots.Length;

            // for all the planters I have seen, the logic is the same: Available slots are numbered starting with the big slots
            if (slot < bigSlotCount)
            {
                // index 0 .. #big-1
                return planter.bigSlots[slot].GetComponentInChildren<GrowingPlant>();
            }
            else
            {
                // index #big .. #big+#small-1
                return planter.slots[slot - bigSlotCount].GetComponentInChildren<GrowingPlant>();
            }
        }
    }
}
