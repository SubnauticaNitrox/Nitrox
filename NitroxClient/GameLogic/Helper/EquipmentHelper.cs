using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class EquipmentHelper
    {
        // Forgive me father for I have sinned
        // someone replace this horrid code.... maybe loop through types and reflect them all OR circumvent the need for this

        public static Optional<Equipment> GetBasedOnOwnersType(GameObject owner)
        {
            Charger charger = owner.GetComponent<Charger>();

            if (charger != null)
            {
                return Optional<Equipment>.Of((Equipment)charger.ReflectionGet("equipment"));
            }

            BaseNuclearReactor baseNuclearReactor = owner.GetComponent<BaseNuclearReactor>();

            if (baseNuclearReactor != null)
            {
                return Optional<Equipment>.Of((Equipment)baseNuclearReactor.ReflectionGet("_equipment"));
            }

            CyclopsDecoyLoadingTube cyclopsDecoyLoadingTube = owner.GetComponent<CyclopsDecoyLoadingTube>();

            if (cyclopsDecoyLoadingTube != null)
            {
                return Optional<Equipment>.Of(cyclopsDecoyLoadingTube.decoySlots);
            }

            Exosuit exosuit = owner.GetComponent<Exosuit>();

            if (exosuit != null)
            {
                return Optional<Equipment>.Of(exosuit.modules);
            }

            SeaMoth seamoth = owner.GetComponent<SeaMoth>();

            if (seamoth != null)
            {
                return Optional<Equipment>.Of(seamoth.modules);
            }

            UpgradeConsole upgradeConsole = owner.GetComponent<UpgradeConsole>();

            if (upgradeConsole != null)
            {
                return Optional<Equipment>.Of(upgradeConsole.modules);
            }

            Vehicle vehicle = owner.GetComponent<Vehicle>();

            if (vehicle != null)
            {
                return Optional<Equipment>.Of(vehicle.modules);
            }

            VehicleUpgradeConsoleInput vehicleUpgradeConsoleInput = owner.GetComponent<VehicleUpgradeConsoleInput>();

            if (vehicleUpgradeConsoleInput != null)
            {
                return Optional<Equipment>.Of(vehicleUpgradeConsoleInput.equipment);
            }

            return Optional<Equipment>.Empty();
        }
    }
}
