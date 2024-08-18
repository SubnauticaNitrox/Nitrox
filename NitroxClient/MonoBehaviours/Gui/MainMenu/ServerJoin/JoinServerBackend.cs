using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;

public static class JoinServerBackend
{
    private static PlayerPreferenceManager preferencesManager;
    private static PlayerPreference activePlayerPreference;
    private static IMultiplayerSession multiplayerSession;

    private static GameObject multiplayerClient;

    private static string serverIp;
    private static int serverPort;

    public static void RequestSessionReservation(string playerName, Color playerColor)
    {
        preferencesManager.SetPreference(serverIp, new PlayerPreference(playerName, playerColor));

        Optional<string> opPassword = MainMenuEnterPasswordPanel.LastEnteredPassword;
        AuthenticationContext authenticationContext = new(playerName, opPassword);

        multiplayerSession.RequestSessionReservation(new PlayerSettings(playerColor.ToDto()), authenticationContext);
    }

    private static void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
    {
        switch (state.CurrentStage)
        {
            case MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY:
                Log.Info("Requesting session policy info");
                Log.InGame(Language.main.Get("Nitrox_RequestingSessionPolicy"));
                break;

            case MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS:
                Color.RGBToHSV(activePlayerPreference.PreferredColor(), out float hue, out float saturation, out float brightness); // HSV => Hue Saturation Value, HSB => Hue Saturation Brightness
                MainMenuJoinServerPanel.Instance.UpdatePlayerPanelValues(activePlayerPreference.PlayerName, new Vector3(hue, saturation, brightness));

                if (multiplayerSession.SessionPolicy.RequiresServerPassword)
                {
                    Log.Info("Waiting for server password input");
                    Log.InGame(Language.main.Get("Nitrox_WaitingPassword"));
                    MainMenuEnterPasswordPanel.ResetLastEnteredPassword();
                    MainMenuRightSide.main.OpenGroup(MainMenuEnterPasswordPanel.NAME);
                    MainMenuEnterPasswordPanel.Instance.FocusPasswordField();
                    break;
                }

                Log.Info("Waiting for user input");
                Log.InGame(Language.main.Get("Nitrox_WaitingUserInput"));
                MainMenuRightSide.main.OpenGroup(MainMenuJoinServerPanel.NAME);
                MainMenuJoinServerPanel.Instance.FocusNameInputField();
                break;

            case MultiplayerSessionConnectionStage.SESSION_RESERVED:
                Log.Info("Launching game");
                Log.InGame(Language.main.Get("Nitrox_LaunchGame"));
                multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                preferencesManager.Save();
                StartGame();
                break;

            case MultiplayerSessionConnectionStage.SESSION_RESERVATION_REJECTED:
                Log.Info("Reservation rejected");
                Log.InGame(Language.main.Get("Nitrox_RejectedSessionPolicy"));

                MultiplayerSessionReservationState reservationState = multiplayerSession.Reservation.ReservationState;

                string reservationRejectionNotification = reservationState.Describe();

                MainMenuNotificationPanel.ShowMessage(reservationRejectionNotification, null, () =>
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

    public static async Task StartMultiplayerClientAsync(IPAddress ip, int port)
    {
        serverIp = ip.ToString();
        serverPort = port;
        NitroxServiceLocator.BeginNewLifetimeScope();

        preferencesManager = NitroxServiceLocator.LocateService<PlayerPreferenceManager>();
        activePlayerPreference = preferencesManager.GetPreference(serverIp);
        multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();

        if (!multiplayerClient)
        {
            multiplayerClient = new GameObject("Nitrox Multiplayer Client");
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
            string msg = $"{Language.main.Get("Nitrox_UnableToConnect")} {serverIp}:{serverPort}";

            if (ip.IsLocalhost())
            {
                if (Process.GetProcessesByName("NitroxServer-Subnautica").Length == 0)
                {
                    Log.Error("No server process was found while address was localhost");
                    msg += $"\n{Language.main.Get("Nitrox_StartServer")}";
                }
                else
                {
                    Log.Error(ex);
                    msg += $"\n{Language.main.Get("Nitrox_FirewallInterfering")}";
                }
            }

            Log.InGame(msg);
            StopMultiplayerClient();
            MainMenuNotificationPanel.ShowMessage(msg, MainMenuServerListPanel.NAME);
        }
    }

    /// <summary>
    /// This method starts a connection with the provided server but leaves handling the session negotiation for the caller.
    /// </summary>
    public static async Task StartDetachedMultiplayerClientAsync(IPAddress ip, int port, MultiplayerSessionConnectionStateChangedEventHandler sessionHandler)
    {
        multiplayerClient = new GameObject("Nitrox Multiplayer Client");
        Task task = StartMultiplayerClientAsync(ip, port);
        multiplayerClient.AddComponent<Multiplayer>();
        multiplayerSession.ConnectionStateChanged += sessionHandler;
        await task;
    }

    public static void StartGame()
    {
#pragma warning disable CS0618 // God Damn it UWE...
        Multiplayer.SubnauticaLoadingStarted();
#if SUBNAUTICA
        IEnumerator startNewGame = uGUI_MainMenu.main.StartNewGame(GameMode.Survival);
#elif BELOWZERO
        IEnumerator startNewGame = uGUI_MainMenu.main.StartNewGame(GameModePresetId.Survival);
#endif
#pragma warning restore CS0618 // God damn it UWE...
        UWE.CoroutineHost.StartCoroutine(startNewGame);
        LoadingScreenVersionText.Initialize();
    }

    public static void StopMultiplayerClient()
    {
        if (!multiplayerClient || !Multiplayer.Main)
        {
            return;
        }

        if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
        {
            multiplayerSession.Disconnect();
        }
        multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;

        Multiplayer.Main.StopCurrentSession();
        NitroxServiceLocator.EndCurrentLifetimeScope(); //Always do this last.

        Object.Destroy(multiplayerClient);
        multiplayerClient = null;
    }
}
