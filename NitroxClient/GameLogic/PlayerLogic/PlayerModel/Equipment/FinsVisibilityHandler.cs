using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class FinsVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject fins;
        private readonly GameObject finsRoot;
#if SUBNAUTICA
        private readonly GameObject chargedFins;
        private readonly GameObject chargedFinsRoot;
        private readonly GameObject glideFins;
        private readonly GameObject glideFinsRoot;
#endif

        public FinsVisibilityHandler(GameObject playerModel)
        {
            fins = playerModel.transform.Find(PlayerEquipmentConstants.FINS_GAME_OBJECT_NAME).gameObject;
            finsRoot = playerModel.transform.Find(PlayerEquipmentConstants.FINS_ROOT_GAME_OBJECT_NAME).gameObject;
#if SUBNAUTICA
            chargedFins = playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_GAME_OBJECT_NAME).gameObject;
            chargedFinsRoot = playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_ROOT_GAME_OBJECT_NAME).gameObject;
            glideFins = playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_GAME_OBJECT_NAME).gameObject;
            glideFinsRoot = playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_ROOT_GAME_OBJECT_NAME).gameObject;
#endif
        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool basicFinsVisible = currentEquipment.Contains(TechType.Fins);
            bool chargedFinsVisible = currentEquipment.Contains(TechType.SwimChargeFins);
#if SUBNAUTICA
            bool glideFinsVisible = currentEquipment.Contains(TechType.UltraGlideFins);
#endif

            fins.SetActive(basicFinsVisible);
            finsRoot.SetActive(basicFinsVisible);
#if SUBNAUTICA
            chargedFins.SetActive(chargedFinsVisible);
            chargedFinsRoot.SetActive(chargedFinsVisible);
            glideFins.SetActive(glideFinsVisible);
            glideFinsRoot.SetActive(glideFinsVisible);
#endif
        }
    }
}
