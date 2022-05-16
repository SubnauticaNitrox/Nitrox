using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class ScubaSuitVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject rebreather;
        private readonly GameObject scuba;
        private readonly GameObject scubaTank;
        private readonly GameObject scubaTankTubes;

        public ScubaSuitVisibilityHandler(GameObject playerModel)
        {
            rebreather = playerModel.transform.Find(PlayerEquipmentConstants.REBREATHER_GAME_OBJECT_NAME).gameObject;
            scuba = playerModel.transform.Find(PlayerEquipmentConstants.SCUBA_ROOT_GAME_OBJECT_NAME).gameObject;
            scubaTank = playerModel.transform.Find(PlayerEquipmentConstants.SCUBA_TANK_GAME_OBJECT_NAME).gameObject;
            scubaTankTubes = playerModel.transform.Find(PlayerEquipmentConstants.SCUBA_TANK_TUBES_GAME_OBJECT_NAME).gameObject;
        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool tankEquipped = currentEquipment.Contains(TechType.Tank) ||
                                currentEquipment.Contains(TechType.DoubleTank) ||
                                currentEquipment.Contains(TechType.HighCapacityTank) ||
                                currentEquipment.Contains(TechType.PlasteelTank);

            bool rebreatherVisible = currentEquipment.Contains(TechType.Rebreather);
            bool radiationHelmetVisible = currentEquipment.Contains(TechType.RadiationHelmet);
            bool tankVisible = tankEquipped && !currentEquipment.Contains(TechType.RadiationSuit);
            bool tubesVisible = (rebreatherVisible || radiationHelmetVisible) && tankVisible;
            bool rootVisible = rebreatherVisible || tankVisible;

            rebreather.SetActive(rebreatherVisible);
            scuba.SetActive(rootVisible);
            scubaTank.SetActive(tankVisible);
            scubaTankTubes.SetActive(tubesVisible);

        }
    }
}
