using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NitroxClient.Persistence;
using NitroxClient.Persistence.Model;
using LitJson;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMultiplayerPanel : MonoBehaviour
    {
        private static MainMenuMultiplayerPanel main;

        private Rect addServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 200);
        private GameObject loadedMultiplayerRef;
        private GameObject savedGamesRef;
        private GameObject deleteButtonRef;
        private GameObject multiplayerButton;
        private Transform savedGameAreaContent;
        private JoinServer joinServer;

        private string serverHostInput;
        private string serverNameInput;
        private string serverPortInput;

        private bool shouldFocus;
        private bool showingAddServer;

        public void Setup(GameObject loadedMultiplayer, GameObject savedGames)
        {
            main = this;
            loadedMultiplayerRef = loadedMultiplayer;
            savedGamesRef = savedGames;

            //This sucks, but the only way around it is to establish a Subnautica resources cache and reference it everywhere we need it.
            //Given recent push-back on elaborate designs, I've just crammed it here until we can all get on the same page as far as code-quality standards are concerned.
            joinServer = new GameObject("NitroxJoinServer").AddComponent<JoinServer>();
            joinServer.Setup(savedGamesRef);

            multiplayerButton = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
            savedGameAreaContent = loadedMultiplayerRef.RequireTransform("Scroll View/Viewport/SavedGameAreaContent");
            deleteButtonRef = savedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>().deleteButton;

            PersistedClientData.InitalizeServerList();

            CreateButton(Language.main.Get("Nitrox_AddServer"), ShowAddServerWindow);
            LoadSavedServers();
        }

        private void CreateButton(string text, UnityAction clickEvent)
        {
            GameObject multiplayerButtonInst = Instantiate(multiplayerButton, savedGameAreaContent, false);
            Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
            txt.GetComponent<Text>().text = text;
            Destroy(txt.GetComponent<TranslationLiveUpdate>());
            Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
            multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
            multiplayerButtonButton.onClick.AddListener(clickEvent);
        }

        private void CreateServerButton(string text, string joinIp, string joinPort, Guid token)
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
                OpenJoinServerMenu(joinIp, joinPort, token);
            });

            GameObject delete = Instantiate(deleteButtonRef, multiplayerButtonInst.transform, false);
            Button deleteButtonButton = delete.GetComponent<Button>();
            deleteButtonButton.onClick = new Button.ButtonClickedEvent();
            deleteButtonButton.onClick.AddListener(() =>
            {
                RemoveServer(multiplayerButtonInst.transform.GetSiblingIndex() - 1);
                Destroy(multiplayerButtonInst);
            });
        }

        private void AddServer(string name, string ip, string port, Guid token)
        {
            PersistedClientData.EmplaceServer(name, ip, port, token);
        }

        private void RemoveServer(int index)
        {
            PersistedClientData.RemoveServerByIndex(index);
        }

        public static void OpenJoinServerMenu(string serverIp, string serverPort, Guid token)
        {
            if (main == null)
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

            main.joinServer.Show(endpoint.Address.ToString(), endpoint.Port, token);
        }

        private void ShowAddServerWindow()
        {
            serverNameInput = "local";
            serverHostInput = "127.0.0.1";
            serverPortInput = "11000";
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
            foreach(SavedServer server in PersistedClientData.GetServers())
            {
                CreateServerButton($"{Language.main.Get("Nitrox_ConnectTo")} <b>{server.Name}</b>\n{server.Ip}:{server.Port}", server.Ip, server.Port, new Guid(server.Token));
            }
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
            Guid token = Guid.NewGuid();
            AddServer(serverNameInput, serverHostInput, serverPortInput, token);
            CreateServerButton($"{Language.main.Get("Nitrox_ConnectTo")} <b>{serverNameInput}</b>\n{serverHostInput}:{serverPortInput}", serverHostInput, serverPortInput, token);
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
    }
}
