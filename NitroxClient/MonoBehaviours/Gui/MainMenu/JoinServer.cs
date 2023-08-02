using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public class JoinServer : MonoBehaviour
{
    private readonly JoinServerJoinWindow joinWindow = new();
    private PlayerPreferenceManager preferencesManager;
    private PlayerPreference activePlayerPreference;
    private IMultiplayerSession multiplayerSession;

    private Rect serverPasswordWindowRect = new(Screen.width / 2 - 250, 200, 500, 200);

    private GameObject multiplayerClient;

    private string serverIp;
    private int serverPort;

    private bool shouldFocus;
    private bool showingPasswordWindow;
    private bool passwordEntered;
    private string serverPassword = string.Empty;

    private GameObject joinServerMenu;
    public string MenuName => joinServerMenu.AliveOrNull()?.name ?? throw new Exception("Menu not yet initialized");

    public void Setup(GameObject saveGameMenu)
    {
        JoinServerServerList.InitializeServerList(saveGameMenu, out joinServerMenu, out RectTransform joinServerBackground);
        UWE.CoroutineUtils.StartCoroutineSmart(joinWindow.Initialize(joinServerBackground, OnJoinClick, () => OnCancelClick(true)));

        DontDestroyOnLoad(gameObject);
        Hide();
    }

    public async Task ShowAsync(string ip, int port)
    {
        NitroxServiceLocator.BeginNewLifetimeScope();
        multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
        preferencesManager = NitroxServiceLocator.LocateService<PlayerPreferenceManager>();

        await joinWindow.IsReady();

        gameObject.SetActive(true);
        serverIp = ip;
        serverPort = port;

        //Set Server IP in info label
        joinWindow.SetIP(serverIp);

        //Initialize elements from preferences
        activePlayerPreference = preferencesManager.GetPreference(serverIp);
        joinWindow.SubscribeColorChanged();

        // HSV => Hue Saturation Value, HSB => Hue Saturation Brightness
        Color.RGBToHSV(activePlayerPreference.PreferredColor(), out float hue, out _, out float brightness);
        joinWindow.SetHSB(new Vector3(hue, 1f, brightness));

        joinWindow.PlayerName = activePlayerPreference.PlayerName;

        await StartMultiplayerClientAsync();
    }

    private void Hide()
    {
        joinWindow.UnsubscribeColorChanged();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS ||
            gameObject.GetComponent<MainMenuNotification>())
        {
            return;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
        {
            OnJoinClick();
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelClick(true);
        }
    }

    private void OnGUI()
    {
        if (showingPasswordWindow)
        {
            serverPasswordWindowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Keyboard),
                serverPasswordWindowRect,
                DoServerPasswordWindow,
                Language.main.Get("Nitrox_JoinServerPasswordHeader")
            );
        }
    }

    private void OnDestroy()
    {
        joinWindow.UnsubscribeColorChanged();
    }

    private void FocusPlayerNameTextBox()
    {
        joinWindow.ActivateInputField();
    }

    private async System.Threading.Tasks.Task StartMultiplayerClientAsync()
    {
        if (!multiplayerClient)
        {
            multiplayerClient = new GameObject("Multiplayer Client");
            multiplayerClient.AddComponent<Multiplayer>();
            multiplayerSession.ConnectionStateChanged += SessionConnectionStateChangedHandler;
        }

        try
        {
            await multiplayerSession.ConnectAsync(serverIp, serverPort);
        }
        catch (ClientConnectionFailedException ex)
        {
            Log.ErrorSensitive("Unable to contact the remote server at: {ip}:{port}", serverIp, serverPort);
            Log.InGame($"{Language.main.Get("Nitrox_UnableToConnect")} {serverIp}:{serverPort}");

            if (serverIp.Equals("127.0.0.1"))
            {
                if (Process.GetProcessesByName("NitroxServer-Subnautica").Length == 0)
                {
                    Log.Error("No server process was found while address was 127.0.0.1");
                    Log.InGame(Language.main.Get("Nitrox_StartServer"));
                }
                else
                {
                    Log.Error(ex);
                    Log.InGame(Language.main.Get("Nitrox_FirewallInterfering"));
                }
            }

            OnCancelClick(false);
        }
    }

    private void OnCancelClick(bool returnToMpMenu)
    {
        StopMultiplayerClient();
        if (returnToMpMenu)
        {
            MainMenuRightSide.main.OpenGroup("Multiplayer");
        }

        Hide();
    }

    private void OnJoinClick()
    {
        string playerName = joinWindow.PlayerName;

        //https://regex101.com/r/eTWiEs/2/
        if (!Regex.IsMatch(playerName, @"^[a-zA-Z0-9._-]{3,25}$"))
        {
            NotifyUser(Language.main.Get("Nitrox_InvalidUserName"));
            return;
        }

        preferencesManager.SetPreference(serverIp, new PlayerPreference(playerName, joinWindow.GetCurrentColor()));

        AuthenticationContext authenticationContext = new(playerName, passwordEntered ? Optional.Of(serverPassword) : Optional.Empty);

        multiplayerSession.RequestSessionReservation(new PlayerSettings(joinWindow.GetCurrentColor().ToDto()), authenticationContext);
    }

    private void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
    {
        switch (state.CurrentStage)
        {
            case MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY:
                Log.Info("Requesting session policy info");
                Log.InGame(Language.main.Get("Nitrox_RequestingSessionPolicy"));
                break;

            case MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS:
                if (multiplayerSession.SessionPolicy.RequiresServerPassword)
                {
                    Log.Info("Waiting for server password input");
                    Log.InGame(Language.main.Get("Nitrox_WaitingPassword"));
                    showingPasswordWindow = true;
                    shouldFocus = true;
                }

                Log.Info("Waiting for user input");
                Log.InGame(Language.main.Get("Nitrox_WaitingUserInput"));
                MainMenuRightSide.main.OpenGroup("Join Server");
                FocusPlayerNameTextBox();
                break;

            case MultiplayerSessionConnectionStage.SESSION_RESERVED:
                Log.Info("Launching game");
                Log.InGame(Language.main.Get("Nitrox_LaunchGame"));
                multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                preferencesManager.Save();

#pragma warning disable CS0618 // God Damn it UWE...
                Multiplayer.SubnauticaLoadingStarted();
                IEnumerator startNewGame = uGUI_MainMenu.main.StartNewGame(GameMode.Survival);
#pragma warning restore CS0618 // God damn it UWE...
                StartCoroutine(startNewGame);
                LoadingScreenVersionText.Initialize();

                break;

            case MultiplayerSessionConnectionStage.SESSION_RESERVATION_REJECTED:
                Log.Info("Reservation rejected");
                Log.InGame(Language.main.Get("Nitrox_RejectedSessionPolicy"));

                MultiplayerSessionReservationState reservationState = multiplayerSession.Reservation.ReservationState;

                string reservationRejectionNotification = reservationState.Describe();

                NotifyUser(
                    reservationRejectionNotification,
                    () =>
                    {
                        multiplayerSession.Disconnect();
                        multiplayerSession.ConnectAsync(serverIp, serverPort);
                    });
                break;

            case MultiplayerSessionConnectionStage.DISCONNECTED:
                Log.Info(Language.main.Get("Nitrox_DisconnectedSession"));
                break;
        }
    }

    private void NotifyUser(string notificationMessage, Action continuationAction = null)
    {
        if (gameObject.GetComponent<MainMenuNotification>())
        {
            return;
        }

        MainMenuNotification notificationDialog = gameObject.AddComponent<MainMenuNotification>();
        notificationDialog.ShowNotification(notificationMessage, () =>
        {
            continuationAction?.Invoke();
            Destroy(gameObject.GetComponent<MainMenuNotification>(), 0.0001f);
        });
    }

    public void StopMultiplayerClient()
    {
        if (!multiplayerClient)
        {
            return;
        }

        Multiplayer.Main.StopCurrentSession();
        Destroy(multiplayerClient);
        multiplayerClient = null;
        if (multiplayerSession != null)
        {
            multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
        }
    }

    private void DoServerPasswordWindow(int windowId)
    {
        GUISkin GetGUISkin() => GUISkinUtils.RegisterDerivedOnce("menus.serverPassword",
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

        Event e = Event.current;
        if (e.isKey)
        {
            switch (e.keyCode)
            {
                case KeyCode.Return:
                    OnSubmitPasswordButtonClicked();
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
                                                GUILayout.Label(Language.main.Get("Nitrox_JoinServerPassword"));
                                                GUI.SetNextControlName("serverPasswordField");
                                                serverPassword = GUILayout.TextField(serverPassword);
                                            }

                                            if (GUILayout.Button(Language.main.Get("Nitrox_SubmitPassword")))
                                            {
                                                HidePasswordWindow();
                                                OnSubmitPasswordButtonClicked();
                                            }

                                            if (GUILayout.Button(Language.main.Get("Nitrox_Cancel")))
                                            {
                                                HidePasswordWindow();
                                                OnCancelClick(true);
                                            }
                                        }
                                    });

        if (shouldFocus)
        {
            GUI.FocusControl("serverPasswordField");
            shouldFocus = false;
        }
    }

    private void OnSubmitPasswordButtonClicked()
    {
        SubmitPassword();
        HidePasswordWindow();
    }

    private void SubmitPassword()
    {
        passwordEntered = true;
    }

    private void OnCancelButtonClicked()
    {
        multiplayerSession.Disconnect();
        HidePasswordWindow();
    }

    private void HidePasswordWindow()
    {
        showingPasswordWindow = false;
        shouldFocus = false;
    }
}
