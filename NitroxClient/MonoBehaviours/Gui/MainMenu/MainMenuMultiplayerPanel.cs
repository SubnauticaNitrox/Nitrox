using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMultiplayerPanel : MonoBehaviour
    {
        public static MainMenuMultiplayerPanel Main;
        private Rect addServerWindowRect = new(Screen.width / 2 - 250, 200, 500, 200);
        private GameObject loadedMultiplayerRef;
        private GameObject savedGamesRef;
        private GameObject deleteButtonRef;
        private GameObject multiplayerButton;
        private Transform savedGameAreaContent;
        public JoinServer JoinServer;

        private string serverHostInput;
        private string serverNameInput;
        private string serverPortInput;

        private bool shouldFocus;
        private bool showingAddServer;

        public void Setup(GameObject loadedMultiplayer, GameObject savedGames)
        {
            Main = this;
            loadedMultiplayerRef = loadedMultiplayer;
            savedGamesRef = savedGames;

            //This sucks, but the only way around it is to establish a Subnautica resources cache and reference it everywhere we need it.
            //Given recent push-back on elaborate designs, I've just crammed it here until we can all get on the same page as far as code-quality standards are concerned.
            JoinServer = new GameObject("NitroxJoinServer").AddComponent<JoinServer>();
            JoinServer.Setup(savedGamesRef);

            multiplayerButton = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
            savedGameAreaContent = loadedMultiplayerRef.RequireTransform("Scroll View/Viewport/SavedGameAreaContent");
            deleteButtonRef = savedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>().deleteButton;

            CreateButton(translationKey: "Nitrox_AddServer", clickEvent: ShowAddServerWindow, disableTranslation: false);
            LoadSavedServers();
            _ = FindLANServersAsync();
        }

        private void CreateButton(string translationKey, UnityAction clickEvent, bool disableTranslation)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton, savedGameAreaContent, false);
            Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
            txt.GetComponent<Text>().text = translationKey;

            if (disableTranslation)
            {
                Destroy(txt.GetComponent<TranslationLiveUpdate>());
            }

            Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
        }

        private void CreateServerButton(string text, string joinIp, string joinPort, bool isReadOnly = false)
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
                OpenJoinServerMenu(joinIp, joinPort);
            });

            // We don't want servers that are discovered automatically to be deleted
            if (!isReadOnly)
            {
                GameObject delete = Instantiate(deleteButtonRef, multiplayerButtonInst.transform, false);
                Button deleteButtonButton = delete.GetComponent<Button>();
                deleteButtonButton.onClick = new Button.ButtonClickedEvent();
                deleteButtonButton.onClick.AddListener(() =>
                {
                    ServerList.Instance.RemoveAt(multiplayerButtonInst.transform.GetSiblingIndex() - 1);
                    ServerList.Instance.Save();
                    Destroy(multiplayerButtonInst);
                });
            }
        }

        public static void OpenJoinServerMenu(string serverIp, string serverPort)
        {
            if (Main == null)
            {
                Log.Error("MainMenuMultiplayerPanel is not instantiated although OpenJoinServerMenu is called.");
                return;
            }
            IPEndPoint endpoint = ResolveIPEndPoint(serverIp, serverPort);
            if (endpoint == null)
            {
                Log.InGame($"{Language.main.Get("Nitrox_UnableToConnect")}: {serverIp}:{serverPort}");
                return;
            }

            Main.JoinServer.Show(endpoint.Address.ToString(), endpoint.Port);
        }

        private void ShowAddServerWindow()
        {
            serverNameInput = "local";
            serverHostInput = "127.0.0.1";
            serverPortInput = ServerList.DEFAULT_PORT.ToString();
            showingAddServer = true;
            shouldFocus = true;
        }

        private void HideAddServerWindow()
        {
            showingAddServer = false;
            shouldFocus = true;
        }

        private void OnGUI()
        {
            if (showingAddServer)
            {
                addServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), addServerWindowRect, DoAddServerWindow, Language.main.Get("Nitrox_AddServer"));
            }
        }

        private void LoadSavedServers()
        {
            ServerList.Refresh();
            foreach (ServerList.Entry entry in ServerList.Instance.Entries)
            {
                CreateServerButton(MakeButtonText(entry.Name, entry.Address, entry.Port), $"{entry.Address}", $"{entry.Port}");
            }
        }

        private async Task FindLANServersAsync()
        {
            void AddButton(IPEndPoint serverEndPoint)
            {
                // Add ServerList entry to keep indices in sync with servers UI, to enable removal by index
                ServerList.Instance.Add(new("LAN Server", $"{serverEndPoint.Address}", $"{serverEndPoint.Port}", false));
                CreateServerButton(MakeButtonText("LAN Server", serverEndPoint.Address, serverEndPoint.Port), $"{serverEndPoint.Address}", $"{serverEndPoint.Port}", true);
            }

            LANDiscoveryClient.ServerFound += AddButton;
            await LANDiscoveryClient.SearchAsync();
            LANDiscoveryClient.ServerFound -= AddButton;
        }

        private string MakeButtonText(string serverName, object address, object port)
        {
            return $"{Language.main.Get("Nitrox_ConnectTo")} <b>{serverName}</b>\n{HideIfNecessary(address)}:{HideIfNecessary(port)}";
        }
        private static string HideIfNecessary(object text)
        {
            return NitroxPrefs.HideIp.Value ? "****" : $"{text}";
        }

        private static IPEndPoint ResolveIPEndPoint(string serverIp, string serverPort)
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

        private static IPAddress ResolveHostName(string hostname, string serverPort)
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
            ServerList.Instance.Add(new ServerList.Entry(serverNameInput, serverHostInput, serverPortInput));
            ServerList.Instance.Save();
            CreateServerButton(MakeButtonText(serverNameInput, serverHostInput, serverPortInput), serverHostInput, serverPortInput);
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
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(Language.main.Get("Nitrox_AddServerName"));
                            GUI.SetNextControlName("serverNameField");
                            // 120 so users can't go too crazy.
                            serverNameInput = GUILayout.TextField(serverNameInput, 120);
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(Language.main.Get("Nitrox_AddServerHost"));
                            GUI.SetNextControlName("serverHostField");
                            // 120 so users can't go too crazy.
                            serverHostInput = GUILayout.TextField(serverHostInput, 120);
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(Language.main.Get("Nitrox_AddServerPort"));
                            GUI.SetNextControlName("serverPortField");
                            serverPortInput = GUILayout.TextField(serverPortInput);
                        }

                        if (GUILayout.Button(Language.main.Get("Nitrox_AddServerAdd")))
                        {
                            OnAddServerButtonClicked();
                        }

                        if (GUILayout.Button(Language.main.Get("Nitrox_Cancel")))
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

        public void RefreshServerEntries()
        {
            if (!savedGameAreaContent)
            {
                return;
            }

            foreach (Transform child in savedGameAreaContent)
            {
                Destroy(child.gameObject);
            }

            CreateButton(translationKey: "Nitrox_AddServer", clickEvent: ShowAddServerWindow, disableTranslation: false);
            LoadSavedServers();
            _ = FindLANServersAsync();
        }
    }
}
