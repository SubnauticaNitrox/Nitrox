using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMultiplayerPanel : MonoBehaviour
    {
        public static MainMenuMultiplayerPanel Main;
        private Rect addServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 200);
        private GameObject joinServerGameObject;
        public GameObject LoadedMultiplayerRef;

        private GameObject multiplayerButton;
        private Transform savedGameAreaContent;
        public GameObject SavedGamesRef;
        public string SERVER_LIST_PATH = Path.Combine(".", "servers");
        public string LAUNCHER_PATH = NitroxModel.Helper.NitroxAppData.Instance.LauncherPath;
        private string serverHostInput;
        private string serverNameInput;
        private string serverPortInput;
        private string serverIdInput = "";

        private bool shouldFocus;
        private bool showingAddServer;

        public void Awake()
        {
            Main = this;
            //This sucks, but the only way around it is to establish a Subnautica resources cache and reference it everywhere we need it.
            //Given recent push-back on elaborate designs, I've just crammed it here until we can all get on the same page as far as code-quality standars are concerned.
            JoinServer.SaveGameMenuPrototype = SavedGamesRef;

            multiplayerButton = SavedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
            savedGameAreaContent = LoadedMultiplayerRef.RequireTransform("Scroll View/Viewport/SavedGameAreaContent");

            if (!File.Exists(SERVER_LIST_PATH))
            {
                AddServer("local server", "127.0.0.1", "11000");
            }

            CreateButton("Add server IP", ShowAddServerWindow);
            LoadSavedServers();
        }

        public void CreateButton(string text, UnityAction clickEvent)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton);
            Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
            txt.GetComponent<Text>().text = text;
            Destroy(txt.GetComponent<TranslationLiveUpdate>());
            Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
            multiplayerButtonInst.transform.SetParent(savedGameAreaContent, false);
        }

        public void CreateServerButton(string text, string joinIp, string joinPort, string serverId = "")
        {
            Task.Factory.StartNew(() => 
            {
                GameObject multiplayerButtonInst = Instantiate(multiplayerButton, savedGameAreaContent, false);
                multiplayerButtonInst.name = (savedGameAreaContent.childCount - 1).ToString();
                Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
                txt.GetComponent<Text>().text = text;
                Color prevTextColor = txt.GetComponent<Text>().color;
                Destroy(txt.GetComponent<TranslationLiveUpdate>());
                Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
                multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
                multiplayerButtonButton.onClick.AddListener(() =>
                {
                    txt.GetComponent<Text>().color = prevTextColor; // Visual fix for black text after click (hover state still active)
                    OpenJoinServerMenu(joinIp, joinPort, serverId);
                });

                GameObject delete = Instantiate(SavedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>().deleteButton);
                Button deleteButtonButton = delete.GetComponent<Button>();
                deleteButtonButton.onClick = new Button.ButtonClickedEvent();
                deleteButtonButton.onClick.AddListener(() =>
                {
                    RemoveServer(multiplayerButtonInst.transform.GetSiblingIndex() - 1);
                    Destroy(multiplayerButtonInst);
                });
                delete.transform.SetParent(multiplayerButtonInst.transform, false);
            });
        }

        public void AddServer(string name, string ip, string port, string serverid = "")
        {
            using (StreamWriter sw = new StreamWriter(SERVER_LIST_PATH, true))
            {
                sw.WriteLine($"{name}|{ip}|{port}|{serverid}");
            }
        }

        public void RemoveServer(int index)
        {
            List<string> serverLines = new List<string>(File.ReadAllLines(SERVER_LIST_PATH));
            serverLines.RemoveAt(index);
            File.WriteAllLines(SERVER_LIST_PATH, serverLines.ToArray());
        }

        public void OpenJoinServerMenu(string serverIp, string serverPort, string serverId = "")
        {
            JoinServerAsync(serverIp, serverPort, serverId);
        }

        public async Task JoinServerAsync(string serverIp, string serverPort, string serverId = "")
        {
            Process JoinNet = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = "zerotiermiddleman join " + serverId,
                    WorkingDirectory = LAUNCHER_PATH,
                    FileName = Path.Combine(LAUNCHER_PATH, "NitroxLauncher.exe")
                }
            };
            JoinNet.Start();
            JoinNet.WaitForExit();

            // delay buffer to prevent joining errors
            await Task.Delay(1500);
            
            IPEndPoint endpoint = ResolveIPEndPoint(serverIp, serverPort);
            if (endpoint == null)
            {
                Log.InGame($"Unable to resolve remote address: {serverIp}:{serverPort}");
                return;
            }

            NitroxServiceLocator.BeginNewLifetimeScope();

            if (joinServerGameObject != null)
            {
                Destroy(joinServerGameObject);
            }

            JoinServer joinServerComponent = new GameObject().AddComponent<JoinServer>();
            joinServerComponent.ServerIp = endpoint.Address.ToString();
            joinServerComponent.ServerPort = endpoint.Port;
        }

        public void ShowAddServerWindow()
        {
            serverNameInput = "local";
            serverHostInput = "127.0.0.1";
            serverPortInput = "11000";
            showingAddServer = true;
            shouldFocus = true;
        }

        public void HideAddServerWindow()
        {
            showingAddServer = false;
            shouldFocus = true;
        }

        public void OnGUI()
        {
            if (!showingAddServer)
            {
                return;
            }
            addServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), addServerWindowRect, DoAddServerWindow, "Add Server");
        }

        private void LoadSavedServers()
        {
            using (StreamReader sr = new StreamReader(SERVER_LIST_PATH))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineData = line.Split('|');
                    bool hasInitialize = false;
                    if(lineData.Length >= 4)
                        if (lineData[3].Length >= 16)
                        {
                            hasInitialize = true;
                            string serverName = lineData[0];
                            string serverDetails = lineData[3];
                            string serverId = serverDetails.Substring(0, 16);
                            string serverIp = "10.10.10." + serverDetails.Substring(16);
                            string serverPort = lineData[2];
                            if (lineData[2].Length < 3)
                            {
                                serverPort = "11000";
                            }
                            CreateServerButton($"Connect to <b>{serverName}</b>\n{serverId}:{serverIp}", serverIp, serverPort, serverId);
                        }
                    if (!hasInitialize)
                    {
                        string serverName = lineData[0];
                        string serverIp = lineData[1];
                        string serverPort;
                        if (lineData.Length == 3)
                        {
                            serverPort = lineData[2];
                        }
                        else
                        {
                            Match match = Regex.Match(serverIp, @"^(.*?)(?::(\d{3,5}))?$");
                            serverIp = match.Groups[1].Value;
                            serverPort = match.Groups[2].Success ? match.Groups[2].Value : "11000";
                        }
                        CreateServerButton($"Connect to <b>{serverName}</b>\n{serverIp}:{serverPort}", serverIp, serverPort);
                    }
                }
            }
        }

        private IPEndPoint ResolveIPEndPoint(string serverIp, string serverPort)
        {
            UriHostNameType hostType = Uri.CheckHostName(serverIp);
            IPAddress address;
            switch (hostType)
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    IPAddress.TryParse(serverIp, out address);
                    break;
                case UriHostNameType.Dns:
                    address = ResolveHostName(serverIp, serverPort);
                    break;
                default:
                    return null;
            }

            if (address == null)
            {
                return null;
            }
            return new IPEndPoint(address, int.Parse(serverPort));
        }

        private IPAddress ResolveHostName(string hostname, string serverPort)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
                return hostEntry.AddressList[0];
            }
            catch (SocketException ex)
            {
                Log.ErrorSensitive(ex, "Unable to resolve the address {hostname}:{serverPort}", hostname, serverPort);
                return null;
            }
        }

        private void OnAddServerButtonClicked()
        {
            serverNameInput = serverNameInput.Trim();
            serverHostInput = serverHostInput.Trim();
            serverPortInput = serverPortInput.Trim();
            serverIdInput = serverIdInput.Trim();
            AddServer(serverNameInput, serverHostInput, serverPortInput, serverIdInput);
            if (serverIdInput.Length > 0)
            {
                string serverId = serverIdInput.Substring(0, 16);
                string serverIp = "10.10.10." + serverIdInput.Substring(16);
                CreateServerButton($"Connect to <b>{serverNameInput}</b>\n{serverId}:{serverIp}", serverIp, "11000", serverId);
            }
            else
            {
                CreateServerButton($"Connect to <b>{serverNameInput}</b>\n{serverHostInput}:{serverPortInput}", serverHostInput, serverPortInput);
            }
            HideAddServerWindow();
        }

        private void OnCancelButtonClicked()
        {
            HideAddServerWindow();
        }

        private GUISkin GetGUISkin()
        {
            return GUISkinUtils.RegisterDerivedOnce("menus.server",
                s =>
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

            GUISkinUtils.RenderWithSkin(GetGUISkin(),
                () =>
                {
                    using (new GUILayout.VerticalScope("Box"))
                    {
                        /*
                        Texture2D BlueTexture = new Texture2D(1, 1);
                        BlueTexture.SetPixel(0, 0, new Color(0.12f, 0.53f, 0.84f));
                        BlueTexture.Apply();
                        GUI.skin.box.normal.background = BlueTexture;
                        */

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Name:");
                            GUI.SetNextControlName("serverNameField");
                            // 120 so users can't go too crazy.
                            serverNameInput = GUILayout.TextField(serverNameInput, 120);
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Server Id:");
                            GUI.SetNextControlName("serverIdField");
                            serverIdInput = GUILayout.TextField(serverIdInput, 19);
                        }
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Port:");
                            GUI.SetNextControlName("serverPortField");
                            serverPortInput = GUILayout.TextField(serverPortInput);
                        }

                        GUILayout.HorizontalSlider(0, 0, 0);

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

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Port:");
                            GUI.SetNextControlName("serverPortField");
                            serverPortInput = GUILayout.TextField(serverPortInput);
                        }

                        GUILayout.Space(20);

                        if (GUILayout.Button("Add server"))
                        {
                            OnAddServerButtonClicked();
                        }

                        GUILayout.Space(5);

                        if (GUILayout.Button("Cancel"))
                        {
                            OnCancelButtonClicked();
                        }
                    }
                });

            if (shouldFocus)
            {
                GUI.FocusControl("serverNameField");
                shouldFocus = false;
            }
        }
    }
}
