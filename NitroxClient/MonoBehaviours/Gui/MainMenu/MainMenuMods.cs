using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.Unity.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMods : MonoBehaviour
    {
        private MainMenuRightSide rightSide;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == "XMenu")
            {
                rightSide = MainMenuRightSide.main;
                MultiplayerMenuMods();
                DiscordClient.InitializeRPMenu();
            }
        }

        private void MultiplayerMenuMods()
        {
            GameObject startButton = GameObjectHelper.RequireGameObject("Menu canvas/Panel/MainMenu/PrimaryOptions/MenuButtons/ButtonPlay");
            GameObject showLoadedMultiplayer = Instantiate(startButton, startButton.transform.parent);
            showLoadedMultiplayer.name = "ButtonMultiplayer";
            showLoadedMultiplayer.transform.SetSiblingIndex(3);

            TextMeshProUGUI buttonText = showLoadedMultiplayer.RequireGameObject("Circle/Bar/Text").GetComponent<TextMeshProUGUI>();
            buttonText.text = Language.main.Get("Nitrox_Multiplayer");
            buttonText.GetComponent<TranslationLiveUpdate>().translationKey = "Nitrox_Multiplayer";


            Button showLoadedMultiplayerButton = showLoadedMultiplayer.GetComponent<Button>();
            showLoadedMultiplayerButton.onClick.RemoveAllListeners();
            showLoadedMultiplayerButton.onClick.AddListener(ShowMultiplayerMenu);

            GameObject savedGamesRef = rightSide.gameObject.RequireGameObject("SavedGames");

            GameObject CloneMainMenuLoadPanel(string panelName, string translationKey)
            {
                GameObject menuPanel = Instantiate(savedGamesRef, rightSide.transform);
                menuPanel.name = panelName;
                Transform header = menuPanel.RequireTransform("Header");
                header.GetComponent<TextMeshProUGUI>().text = Language.main.Get(translationKey);
                header.GetComponent<TranslationLiveUpdate>().translationKey = translationKey;
                Destroy(menuPanel.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame"));
                Destroy(menuPanel.GetComponent<MainMenuLoadPanel>());
                Destroy(menuPanel.GetComponentInChildren<MainMenuLoadMenu>());

                rightSide.groups.Add(menuPanel.GetComponent<MainMenuGroup>());
                return menuPanel;
            }

            GameObject serverList = CloneMainMenuLoadPanel("MultiplayerServerList", "Nitrox_Multiplayer");
            serverList.AddComponent<MainMenuMultiplayerPanel>().Setup(savedGamesRef);

            GameObject serverCreate = CloneMainMenuLoadPanel("MultiplayerCreateServer", "Nitrox_AddServer");
            serverCreate.AddComponent<MainMenuCreateServerPanel>().Setup(savedGamesRef);


#if RELEASE
            // Remove singleplayer button because SP is broken when Nitrox is injected. TODO: Allow SP to work and co-exist with Nitrox MP in the future
            startButton.SetActive(false);
#endif
        }

        private void ShowMultiplayerMenu()
        {
            rightSide.OpenGroup("MultiplayerServerList");
        }
    }
}
