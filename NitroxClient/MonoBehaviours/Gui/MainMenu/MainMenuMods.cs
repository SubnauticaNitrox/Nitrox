using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMods : MonoBehaviour
    {

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
                MultiplayerMenuMods();
            }
        }

        private void MultiplayerMenuMods()
        {
            GameObject startButton = GameObject.Find("Menu canvas/Panel/MainMenu/PrimaryOptions/MenuButtons/ButtonPlay");
            GameObject showLoadedMultiplayer = Instantiate(startButton);
            Text buttonText = showLoadedMultiplayer.transform.Find("Circle/Bar/Text").gameObject.GetComponent<Text>();
            buttonText.text = "Multiplayer";
            showLoadedMultiplayer.transform.SetParent(GameObject.Find("Menu canvas/Panel/MainMenu/PrimaryOptions/MenuButtons").transform, false);
            showLoadedMultiplayer.transform.SetSiblingIndex(3);
            Button showLoadedMultiplayerButton = showLoadedMultiplayer.GetComponent<Button>();
            showLoadedMultiplayerButton.onClick.RemoveAllListeners();
            showLoadedMultiplayerButton.onClick.AddListener(ShowMultiplayerMenu);

            MainMenuRightSide rightSide = MainMenuRightSide.main;
            GameObject savedGamesRef = FindObject(rightSide.gameObject, "SavedGames");
            GameObject LoadedMultiplayer = Instantiate(savedGamesRef);
            LoadedMultiplayer.name = "Multiplayer";
            LoadedMultiplayer.transform.Find("Header").GetComponent<Text>().text = "Multiplayer";
            Destroy(LoadedMultiplayer.transform.Find("SavedGameArea/SavedGameAreaContent/NewGame").gameObject);

            MainMenuMultiplayerPanel panel = LoadedMultiplayer.AddComponent<MainMenuMultiplayerPanel>();
            panel.SavedGamesRef = savedGamesRef;
            panel.LoadedMultiplayerRef = LoadedMultiplayer;

            Destroy(LoadedMultiplayer.GetComponent<MainMenuLoadPanel>());
            LoadedMultiplayer.transform.SetParent(rightSide.transform, false);
            rightSide.groups.Add(LoadedMultiplayer);
        }

        private void ShowMultiplayerMenu()
        {
            MainMenuRightSide rightSide = MainMenuRightSide.main;
            rightSide.OpenGroup("Multiplayer");
        }

        private GameObject FindObject(GameObject parent, string name)
        {
            Component[] trs = parent.GetComponentsInChildren(typeof(Transform), true);
            foreach (Component t in trs)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            return null;
        }
    }
}
