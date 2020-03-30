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
            o => (Equipment)o.GetComponent<Charger>().IfAlive(charger => charger.ReflectionGet("equipment")),
            o => (Equipment)o.GetComponent<BaseNuclearReactor>().IfAlive(reactor => reactor.ReflectionGet("_equipment")),
            o => o.GetComponent<CyclopsDecoyLoadingTube>().IfAlive(decoyTube => decoyTube.decoySlots),
            o => o.GetComponent<Exosuit>().IfAlive(exosuit => exosuit.modules),
            o => o.GetComponent<SeaMoth>().IfAlive(seamoth => seamoth.modules),
            o => o.GetComponent<UpgradeConsole>().IfAlive(console => console.modules),
            o => o.GetComponent<Vehicle>().IfAlive(vehicle => vehicle.modules),
            o => o.GetComponent<VehicleUpgradeConsoleInput>().IfAlive(vehicleConsole => vehicleConsole.equipment),
            o => o.GetComponent<Player>().IfAlive(player => "Player".Equals(player.name, StringComparison.InvariantCulture) ? Inventory.main.equipment : null)
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
