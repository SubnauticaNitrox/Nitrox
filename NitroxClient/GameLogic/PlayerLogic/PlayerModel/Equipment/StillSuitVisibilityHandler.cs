using System.Collections.ObjectModel;
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
        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool bodyVisible = currentEquipment.Contains(TechType.Stillsuit);

            stillSuit.SetActive(bodyVisible);
        }
    }
}
