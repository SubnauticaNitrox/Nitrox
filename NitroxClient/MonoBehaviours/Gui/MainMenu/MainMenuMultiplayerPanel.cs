using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
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
            new GameObject().AddComponent<JoinServer>().serverIp = serverIp;
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
}
