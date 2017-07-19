using NitroxModel.Helper;
using NitroxClient.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UWE;
using NitroxClient.Communication;

namespace NitroxClient.MonoBehaviours
{
    public class MultiplayerButton : MonoBehaviour
    {
        public void Awake()
        {
            MultiplayerMenuMods();
        }

        private void MultiplayerMenuMods()
        {
            GameObject startButton = GameObject.Find("Menu canvas/Panel/MainMenu/PrimaryOptions/MenuButtons/ButtonPlay");
            GameObject showLoadedMultiplayer = Instantiate(startButton);
            Text buttonText = showLoadedMultiplayer.transform.Find("Circle/Bar/Text").gameObject.GetComponent<Text>() as Text;
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
            panel.savedGamesRef = savedGamesRef;
            panel.loadedMultiplayerRef = LoadedMultiplayer;
            
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

    public class MainMenuMultiplayerPanel : MonoBehaviour
    {
        public static readonly string SERVER_LIST_PATH = @".\servers";
        public GameObject savedGamesRef;
        public GameObject loadedMultiplayerRef;

        bool showingAddServer = false;
        string serverInput = "server name";
        string ipInput = "server ip";

        GameObject multiplayerButton;
        Transform savedGameAreaContent;

        public void Awake()
        {
            multiplayerButton = savedGamesRef.transform.Find("SavedGameArea/SavedGameAreaContent/NewGame").gameObject;
            savedGameAreaContent = loadedMultiplayerRef.transform.Find("SavedGameArea/SavedGameAreaContent");
            if (!File.Exists(SERVER_LIST_PATH))
            {
                using (StreamWriter sw = File.CreateText(SERVER_LIST_PATH))
                {
                    sw.WriteLine("local|127.0.0.1");
                }
            }
            CreateServerButton("Add a server", CreateNewServer);
            using (StreamReader sr = new StreamReader(SERVER_LIST_PATH))
            {
                string lineData;
                while ((lineData = sr.ReadLine()) != null)
                {
                    string serverName = lineData.Split('|')[0];
                    string serverIp = lineData.Split('|')[1];
                    int serverIndex = savedGameAreaContent.childCount;
                    CreateServerButton($"<b>{serverName}</b>\n{serverIp}", delegate { JoinServer(serverIp); }, delegate { RemoveServer(serverIndex); });
                }
            }
        }

        public void CreateServerButton(string text, UnityEngine.Events.UnityAction clickEvent)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            multiplayerButtonInst.transform.Find("NewGameButton/Text").GetComponent<Text>().text = text;
            Button multiplayerButtonButton = multiplayerButtonInst.transform.Find("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);
        }

        public void CreateServerButton(string text, UnityEngine.Events.UnityAction clickEvent, UnityEngine.Events.UnityAction deleteEvent)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            multiplayerButtonInst.transform.Find("NewGameButton/Text").GetComponent<Text>().text = text;
            Button multiplayerButtonButton = multiplayerButtonInst.transform.Find("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);

            GameObject delete = Instantiate(savedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>().deleteButton);
            Button deleteButtonButton = delete.GetComponent<Button>();
            deleteButtonButton.onClick = new Button.ButtonClickedEvent();
            deleteButtonButton.onClick.AddListener(deleteEvent);
            delete.transform.SetParent(multiplayerButtonInst.transform, false);
        }

        public void JoinServer(string serverIp)
        {
            new GameObject().AddComponent<JoinServerScript>().serverIp = serverIp;
        }

        public void CreateNewServer()
        {
            showingAddServer = true;
        }

        public void AddServer()
        {
            using (StreamWriter sw = new StreamWriter(SERVER_LIST_PATH, true))
            {
                sw.WriteLine($"{serverInput}|{ipInput}");
            }
        }

        public void RemoveServer(int index)
        {
            List<string> serverLines = new List<string>(File.ReadAllLines(SERVER_LIST_PATH));
            serverLines.RemoveAt(index - 1);
            File.WriteAllLines(SERVER_LIST_PATH, serverLines.ToArray());
            Destroy(savedGameAreaContent.GetChild(index).gameObject);
        }

        public void HideNewServer()
        {
            serverInput = "server name";
            ipInput = "server ip";
            showingAddServer = false;
        }

        public void OnGUI()
        {
            if (!showingAddServer)
            {
                return;
            }
            serverInput = GUI.TextField(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 50, 500, 50), serverInput);
            ipInput = GUI.TextField(new Rect(Screen.width / 2 - 250, Screen.height / 2, 500, 50), ipInput);
            if (GUI.Button(new Rect(Screen.width / 2 - 250, Screen.height / 2 + 50, 500, 50), "Add server"))
            {
                AddServer();
                int serverIndex = savedGameAreaContent.childCount;
                CreateServerButton($"<b>{serverInput}</b>\n{ipInput}", delegate { JoinServer(ipInput); }, delegate { RemoveServer(serverIndex); });
                HideNewServer();
            }
            if (GUI.Button(new Rect(Screen.width / 2 - 250, Screen.height / 2 + 100, 500, 50), "Cancel"))
            {
                HideNewServer();
            }
        }
    }

    public class JoinServerScript : MonoBehaviour
    {
        public string serverIp = "";
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            StartCoroutine(JoinServerWait(serverIp));
        }
        
        public IEnumerator JoinServerWait(string serverIp)
        {
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, GameMode.Creative);
            StartCoroutine(startNewGame);
            //Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);
            Multiplayer.PacketSender.PlayerId = "Player" + UnityEngine.Random.Range(100f,999f);
            Multiplayer.main.StartMultiplayer(serverIp);
            Multiplayer.main.InitMonoBehaviours();
        }
    }
}
