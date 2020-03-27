using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class EquipmentHelper
    {
        private static readonly List<Func<GameObject, Equipment>> equipmentFinders = new List<Func<GameObject, Equipment>>
        {
            o => (Equipment)o.GetComponent<Charger>()?.ReflectionGet("equipment"),
            o => (Equipment)o.GetComponent<BaseNuclearReactor>()?.ReflectionGet("_equipment"),
            o => o.GetComponent<CyclopsDecoyLoadingTube>()?.decoySlots,
            o => o.GetComponent<Exosuit>()?.modules,
            o => o.GetComponent<SeaMoth>()?.modules,
            o => o.GetComponent<UpgradeConsole>()?.modules,
            o => o.GetComponent<Vehicle>()?.modules,
            o => o.GetComponent<VehicleUpgradeConsoleInput>()?.equipment,
            o =>
            {
                Player playerComponent = o.GetComponent<Player>();
                return "Player".Equals(playerComponent.name, StringComparison.InvariantCulture) ? Inventory.main.equipment : null;
            }
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
