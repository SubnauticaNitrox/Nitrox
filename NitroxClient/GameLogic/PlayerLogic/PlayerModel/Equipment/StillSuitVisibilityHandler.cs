using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class StillSuitVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject stillSuit;

        public StillSuitVisibilityHandler(GameObject playerModel)
        {
            stillSuit = playerModel.transform.Find(PlayerEquipmentConstants.STILL_SUIT_GAME_OBJECT_NAME).gameObject;
        }
        public void UpdateEquipmentVisibility(IReadOnlyList<TechType> currentEquipment)
        {
            bool bodyVisible = currentEquipment.Contains(TechType.WaterFiltrationSuit);

            stillSuit.SetActive(bodyVisible);
        }
    }
}
