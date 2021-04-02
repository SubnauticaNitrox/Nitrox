using System.Collections.ObjectModel;

namespace NitroxClient.GameLogic.PlayerModel.Equipment.Abstract
{
    public interface IEquipmentVisibilityHandler
    {
        void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment);
    }
}
