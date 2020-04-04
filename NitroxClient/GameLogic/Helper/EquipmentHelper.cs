using System;
using System.Collections.Generic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class EquipmentHelper
    {
        private static readonly List<Func<GameObject, Equipment>> equipmentFinders = new List<Func<GameObject, Equipment>>
        {
            o => (Equipment)o.GetComponent<Charger>().AliveOrNull()?.ReflectionGet("equipment"),
            o => (Equipment)o.GetComponent<BaseNuclearReactor>().AliveOrNull()?.ReflectionGet("_equipment"),
            o => o.GetComponent<CyclopsDecoyLoadingTube>().AliveOrNull()?.decoySlots,
            o => o.GetComponent<Exosuit>().AliveOrNull()?.modules,
            o => o.GetComponent<SeaMoth>().AliveOrNull()?.modules,
            o => o.GetComponent<UpgradeConsole>().AliveOrNull()?.modules,
            o => o.GetComponent<Vehicle>().AliveOrNull()?.modules,
            o => o.GetComponent<VehicleUpgradeConsoleInput>().AliveOrNull()?.equipment,
            o => string.Equals("Player", o.GetComponent<Player>().AliveOrNull()?.name, StringComparison.InvariantCulture) ? Inventory.main.equipment : null
        };

        public static Optional<Equipment> FindEquipmentComponent(GameObject owner)
        {
            foreach (Func<GameObject, Equipment> equipmentFinder in equipmentFinders)
            {
                Equipment equipment = equipmentFinder(owner);
                if (equipment != null)
                {
                    return Optional.Of(equipment);
                }
            }
            return Optional.Empty;
        }
    }
}
