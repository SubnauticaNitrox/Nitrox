using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.Unity.Helper;
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

            Text buttonText = showLoadedMultiplayer.RequireGameObject("Circle/Bar/Text").GetComponent<Text>();
            buttonText.text = Language.main.Get("Nitrox_Multiplayer");
            buttonText.GetComponent<TranslationLiveUpdate>().translationKey = "Nitrox_Multiplayer";


            Button showLoadedMultiplayerButton = showLoadedMultiplayer.GetComponent<Button>();
            showLoadedMultiplayerButton.onClick.RemoveAllListeners();
            showLoadedMultiplayerButton.onClick.AddListener(ShowMultiplayerMenu);

            GameObject savedGamesRef = rightSide.gameObject.RequireGameObject("SavedGames");
            GameObject loadedMultiplayer = Instantiate(savedGamesRef, rightSide.transform);
            loadedMultiplayer.name = "Multiplayer";
            Transform header = loadedMultiplayer.RequireTransform("Header");
            header.GetComponent<Text>().text = Language.main.Get("Nitrox_Multiplayer");
            header.GetComponent<TranslationLiveUpdate>().translationKey = "Nitrox_Multiplayer";
            Destroy(loadedMultiplayer.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame"));
            Destroy(loadedMultiplayer.GetComponent<MainMenuLoadPanel>());

            loadedMultiplayer.AddComponent<MainMenuMultiplayerPanel>().Setup(loadedMultiplayer, savedGamesRef);

            rightSide.groups.Add(loadedMultiplayer.GetComponent<MainMenuGroup>());

#if RELEASE
            // Remove singleplayer button because SP is broken when Nitrox is injected. TODO: Allow SP to work and co-exist with Nitrox MP in the future
            startButton.SetActive(false);
#endif
        }

        private void ShowMultiplayerMenu()
        {
            rightSide.OpenGroup("Multiplayer");
        }
    }
}
