using System.Collections.Generic;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract
{
    public interface IEquipmentVisibilityHandler
    {
        void UpdateEquipmentVisibility(IReadOnlyList<TechType> currentEquipment);
    }
}
