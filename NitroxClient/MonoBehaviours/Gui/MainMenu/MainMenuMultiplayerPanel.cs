using System.Collections.Generic;
using System.IO;
using System;
using System.Net;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Threading;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMultiplayerPanel : MonoBehaviour
    {
        public string SERVER_LIST_PATH = Path.Combine(".", "servers");
        private Rect addServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 200);
        private GameObject joinServerGameObject;
        public GameObject LoadedMultiplayerRef;

        private GameObject multiplayerButton;
        private Transform savedGameAreaContent;
        public GameObject SavedGamesRef;
        private string serverHostInput;
        private string serverNameInput;

        private bool addServerOnFocus = false;
        private bool showingAddServer = false;

        private bool onlineServersOnFocus = false;
        private bool showingOnlineServers = false;
        private Rect onlineServerWindowRect = new Rect(50, 50, 1000, 600);
        private string serverListText = "";
        private List<ServerInfo> serverInfoList = new List<ServerInfo>();
        private string serverFilter = "";
        private bool serverLoadFinished = false;
        private Vector2 scrollPos = new Vector2();
        private int serverQueryThreadedNum = 0;
        private readonly string serverInfoLink = "http://nitrox.qaq.link/server";

        public void Awake()
        {
            //This sucks, but the only way around it is to establish a Subnautica resources cache and reference it everywhere we need it.
            //Given recent push-back on elaborate designs, I've just crammed it here until we can all get on the same page as far as code-quality standars are concerned.
            JoinServer.SaveGameMenuPrototype = SavedGamesRef;

            multiplayerButton = SavedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
            savedGameAreaContent = LoadedMultiplayerRef.RequireTransform("Scroll View/Viewport/SavedGameAreaContent");

            if (!File.Exists(SERVER_LIST_PATH))
            {
                AddServer("local", "127.0.0.1");
            }

            CreateButton("<color=red>Online Servers</color>", ToggleOnlineServers);
            CreateButton("Add server IP", ShowAddServerWindow);
            LoadSavedServers();
        }

        public void CreateButton(string text, UnityAction clickEvent)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
            txt.GetComponent<Text>().text = text;
            DestroyObject(txt.GetComponent<TranslationLiveUpdate>());
            Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);
        }

        public void CreateServerButton(string text, ServerInfo sinfo)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            multiplayerButtonInst.name = (savedGameAreaContent.childCount - 1).ToString();
            Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
            txt.GetComponent<Text>().text = text;
            DestroyObject(txt.GetComponent<TranslationLiveUpdate>());
            Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(() => OpenJoinServerMenu(sinfo));
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);

            GameObject delete = Instantiate(SavedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>().deleteButton);
            Button deleteButtonButton = delete.GetComponent<Button>();
            deleteButtonButton.onClick = new Button.ButtonClickedEvent();
            deleteButtonButton.onClick.AddListener(() =>
            {
                RemoveServer(multiplayerButtonInst.transform.GetSiblingIndex() - 2);
                Destroy(multiplayerButtonInst);
            });
            delete.transform.SetParent(multiplayerButtonInst.transform, false);
        }

        public void AddServer(string name, string ip)
        {
            using (StreamWriter sw = new StreamWriter(SERVER_LIST_PATH, true))
            {
                sw.WriteLine($"{name}|{ip}");
            }
        }

        public void RemoveServer(int index)
        {
            List<string> serverLines = new List<string>(File.ReadAllLines(SERVER_LIST_PATH));
            serverLines.RemoveAt(index);
            File.WriteAllLines(SERVER_LIST_PATH, serverLines.ToArray());
        }

        public void OpenJoinServerMenu(ServerInfo serverInfo)
        {
            NitroxServiceLocator.BeginNewLifetimeScope();

            if (joinServerGameObject != null)
            {
                Destroy(joinServerGameObject);
            }

            joinServerGameObject = new GameObject();
            JoinServer joinServerComponent = joinServerGameObject.AddComponent<JoinServer>();
            joinServerComponent.ServerIp = serverInfo.Ip;
            joinServerComponent.serverPort = serverInfo.Port;

        }

        public void ShowAddServerWindow()
        {
            serverNameInput = "local";
            serverHostInput = "127.0.0.1";
            showingAddServer = true;
            addServerOnFocus = true;
        }

        public void HideAddServerWindow()
        {
            showingAddServer = false;
            addServerOnFocus = true;
        }

        public void ToggleOnlineServers()
        {
            showingOnlineServers = !showingOnlineServers;
            onlineServersOnFocus = !onlineServersOnFocus;
        }

        public void OnGUI()
        {
            if (showingAddServer)
            {
                addServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), addServerWindowRect, DoAddServerWindow, "Add server");
            }
            if (showingOnlineServers)
            {
                onlineServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), onlineServerWindowRect, DoOnlineServersWindow, "Online Servers");
            }

        }

        private void LoadSavedServers()
        {
            using (StreamReader sr = new StreamReader(SERVER_LIST_PATH))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    ServerInfo sinfo = new ServerInfo(line);
                    CreateServerButton($"Connect to <b>{sinfo.Name}</b>\n{sinfo.Address}", sinfo);
                }
            }
        }


        private void OnAddServerButtonClicked()
        {
            AddServer(serverNameInput, serverHostInput);
            CreateServerButton($"Connect to <b>{serverNameInput}</b>\n{serverHostInput}", new ServerInfo($"{serverNameInput}|{serverHostInput}"));
            HideAddServerWindow();
        }

        private void OnCancelButtonClicked()
        {
            HideAddServerWindow();
        }

        private GUISkin GetGUISkin()
        {
            return GUISkinUtils.RegisterDerivedOnce("menus.server", s =>
            {
                s.textField.fontSize = 14;
                s.textField.richText = false;
                s.textField.alignment = TextAnchor.MiddleLeft;
                s.textField.wordWrap = true;
                s.textField.stretchHeight = true;
                s.textField.padding = new RectOffset(10, 10, 5, 5);

                s.label.fontSize = 14;
                s.label.alignment = TextAnchor.MiddleRight;
                s.label.stretchHeight = true;
                s.label.fixedWidth = 80; //change this when adding new labels that need more space.

                s.button.fontSize = 14;
                s.button.stretchHeight = true;
                
            });
        }

        private GUISkin GetServerListGUISkin()
        {
            return GUISkinUtils.RegisterDerivedOnce("menus.serverlist", s =>
            {
                s.textField.fontSize = 14;
                s.textField.richText = false;
                s.textField.alignment = TextAnchor.MiddleLeft;
                s.textField.wordWrap = false;
                s.textField.stretchHeight = false;
                s.textField.padding = new RectOffset(10, 10, 5, 5);

                s.label.fontSize = 14;
                s.label.alignment = TextAnchor.MiddleLeft;
                s.label.stretchHeight = false;
                s.label.wordWrap = false;

                s.button.fontSize = 14;
                s.button.stretchHeight = false;

            });
        }


        private void DoAddServerWindow(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        OnAddServerButtonClicked();
                        break;
                    case KeyCode.Escape:
                        OnCancelButtonClicked();
                        break;
                }
            }

            GUI.DragWindow(new Rect(0, 0, 500, 20));

            GUISkinUtils.RenderWithSkin(GetGUISkin(), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Name:");
                        GUI.SetNextControlName("serverNameField");
                        // 120 so users can't go too crazy.
                        serverNameInput = GUILayout.TextField(serverNameInput, 120);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Host:");
                        GUI.SetNextControlName("serverHostField");
                        // 120 so users can't go too crazy.
                        serverHostInput = GUILayout.TextField(serverHostInput, 120);
                    }

                    if (GUILayout.Button("Add server"))
                    {
                        OnAddServerButtonClicked();
                    }

                    if (GUILayout.Button("Cancel"))
                    {
                        OnCancelButtonClicked();
                    }
                }
            });

            if (addServerOnFocus)
            {
                GUI.FocusControl("serverNameField");
                addServerOnFocus = false;
            }
        }
        

        private void checkServerInfoThreaded(string serverStr)
        {
            if(serverStr == "" || !serverStr.Contains("|"))
            {
                serverQueryThreadedNum -= 1;
                return;
            }

            new Thread(new ThreadStart(() =>
            {
                while (serverQueryThreadedNum>=20)
                {
                    Thread.Sleep(50);
                }
                ServerInfo info = new ServerInfo(serverStr.TrimEnd('\r'));
                info.UpdateLatency();
                serverQueryThreadedNum -= 1;
                if (info.Ping == -1)
                {
                    return;
                }
                lock (serverInfoList)
                {
                    serverInfoList.Add(info);
                    serverInfoList.Sort();
                }
                
            })).Start();

        }


        private void DoOnlineServersWindow(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        serverLoadFinished = false;
                        break;

                    case KeyCode.Escape:
                        OnCancelButtonClicked();
                        break;
                }
            }

            GUI.DragWindow(new Rect(0, 0, 1000, 20));

            GUISkinUtils.RenderWithSkin(GetServerListGUISkin(), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(" Search:", GUILayout.Width(200));
                        GUI.SetNextControlName("serverFilter");
                        serverFilter = GUILayout.TextField(serverFilter, 120, GUILayout.Width(400));

                        if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                        {
                            serverLoadFinished = false;
                        }
                        if (GUILayout.Button("Close", GUILayout.Width(100)))
                        {
                            showingOnlineServers = false;
                        }
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(" Server Name", GUILayout.Width(400));
                        GUILayout.Label(" Server Address", GUILayout.Width(300));
                        GUILayout.Label(" Latency", GUILayout.Width(100));
                        GUILayout.Label(" Connect");
                    }

                    using (var sv = new GUILayout.ScrollViewScope(scrollPos))
                    {
                        scrollPos = sv.scrollPosition;

                        if (!serverLoadFinished)
                        {
                            if (serverListText == "")
                            {
                                try
                                {
                                    WebClient MyWebClient = new WebClient();
                                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;
                                    byte[] pageData = MyWebClient.DownloadData(serverInfoLink);
                                    serverListText = System.Text.Encoding.UTF8.GetString(pageData);
                                }
                                catch(Exception)
                                {
                                    serverListText = "";
                                }
                            }

                            serverInfoList.Clear();

                            foreach (string i in serverListText.Split('\n'))
                            {
                                serverQueryThreadedNum += 1;
                                checkServerInfoThreaded(i);
                            }
                            serverLoadFinished = true;
                        }
                        lock (serverInfoList)
                        { 
                            foreach (ServerInfo sinfo in serverInfoList)
                            {
                                if (serverFilter != "" &&
                                    sinfo.Name.IndexOf(serverFilter, StringComparison.OrdinalIgnoreCase) < 0 &&
                                    sinfo.Address.IndexOf(serverFilter, StringComparison.OrdinalIgnoreCase) < 0)
                                {
                                    continue;
                                }
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label(sinfo.Name, GUILayout.Width(400));
                                    GUILayout.Label(sinfo.Address, GUILayout.Width(300));

                                    string color = "green";
                                    if (sinfo.Ping < 100)
                                        color = "green";
                                    else if (sinfo.Ping < 200)
                                        color = "yellow";
                                    else
                                        color = "red";
                                    
                                    if(sinfo.Ping == 9999)
                                        GUILayout.Label($"<color=red>Unknown</color>", GUILayout.Width(100));
                                    else
                                        GUILayout.Label($"<color={color}>{sinfo.Ping}ms</color>", GUILayout.Width(100));

                                    if (GUILayout.Button("Connect"))
                                    {
                                        showingOnlineServers = false;
                                        OpenJoinServerMenu(sinfo);
                                    }
                                }

                            }
                        }
                    }

                }
            });
        }

        

    }
}
