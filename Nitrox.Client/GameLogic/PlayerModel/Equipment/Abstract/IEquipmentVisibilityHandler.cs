using System.Collections.ObjectModel;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.Equipment.Abstract
{
    public interface IEquipmentVisibilityHandler
    {
        void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment);
    }
}
