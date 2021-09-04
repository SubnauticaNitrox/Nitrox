using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Equipment
{
    public class FinsVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject fins;
#if SUBNAUTICA
        private readonly GameObject finsRoot;
        private readonly GameObject chargedFins;
        private readonly GameObject chargedFinsRoot;
        private readonly GameObject glideFins;
        private readonly GameObject glideFinsRoot;
#endif

        public FinsVisibilityHandler(GameObject playerModel)
        {
            fins = playerModel.transform.Find(PlayerEquipmentConstants.FINS_GAME_OBJECT_NAME).gameObject;
#if SUBNAUTICA
            finsRoot = playerModel.transform.Find(PlayerEquipmentConstants.FINS_ROOT_GAME_OBJECT_NAME).gameObject;
            chargedFins = playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_GAME_OBJECT_NAME).gameObject;
            chargedFinsRoot = playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_ROOT_GAME_OBJECT_NAME).gameObject;
            glideFins = playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_GAME_OBJECT_NAME).gameObject;
            glideFinsRoot = playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_ROOT_GAME_OBJECT_NAME).gameObject;
#endif
        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool basicFinsVisible = currentEquipment.Contains(TechType.Fins);
#if SUBNAUTICA
            bool chargedFinsVisible = currentEquipment.Contains(TechType.SwimChargeFins);
            bool glideFinsVisible = currentEquipment.Contains(TechType.UltraGlideFins);
#endif

            fins.SetActive(basicFinsVisible);
#if SUBNAUTICA
            finsRoot.SetActive(basicFinsVisible);
            chargedFins.SetActive(chargedFinsVisible);
            chargedFinsRoot.SetActive(chargedFinsVisible);
            glideFins.SetActive(glideFinsVisible);
            glideFinsRoot.SetActive(glideFinsVisible);
#endif
        }
    }
}
