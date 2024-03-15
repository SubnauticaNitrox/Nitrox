using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;

public class JoinServerBackend : MonoBehaviour
{
    private static JoinServerBackend instance;

    public static JoinServerBackend Instance
    {
        get
        {
            if (!instance)
            {
                GameObject persistentGameobject = GameObject.Find("Nitrox");
                instance = persistentGameobject.EnsureComponent<JoinServerBackend>();
            }
            return instance;
        }
    }

    private PlayerPreferenceManager preferencesManager;
    private PlayerPreference activePlayerPreference;
    private IMultiplayerSession multiplayerSession;

    private string serverIp;
    private int serverPort;

    public async Task ShowAsync(string ip, int port)
    {
        serverIp = ip;
        serverPort = port;
        NitroxServiceLocator.BeginNewLifetimeScope();

        preferencesManager = NitroxServiceLocator.LocateService<PlayerPreferenceManager>();
        activePlayerPreference = preferencesManager.GetPreference(serverIp);

        multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();

        gameObject.SetActive(true);
        await StartMultiplayerClientAsync();
    }

    private GameObject multiplayerClient;

    private async Task StartMultiplayerClientAsync()
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
            string msg = $"{Language.main.Get("Nitrox_UnableToConnect")} {serverIp}:{serverPort}";

            if (serverIp.Equals("127.0.0.1"))
            {
                if (Process.GetProcessesByName("NitroxServer-Subnautica").Length == 0)
                {
                    Log.Error("No server process was found while address was 127.0.0.1");
                    msg += $"\n{Language.main.Get("Nitrox_StartServer")}";
                }
                else
                {
                    Log.Error(ex);
                    msg += $"\n{Language.main.Get("Nitrox_FirewallInterfering")}";
                }
            }

            Log.InGame(msg);
            StopMultiplayerClient(msg);
            MainMenuNotificationPanel.ShowMessage(msg, MainMenuServerListPanel.NAME);
        }
    }

    public void RequestSessionReservation(string playerName, Color playerColor)
    {
        preferencesManager.SetPreference(serverIp, new PlayerPreference(playerName, playerColor));

        Optional<string> opPassword = MainMenuEnterPasswordPanel.LastEnteredPassword;
        AuthenticationContext authenticationContext = new(playerName, opPassword);

        multiplayerSession.RequestSessionReservation(new PlayerSettings(playerColor.ToDto()), authenticationContext);
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
                Color.RGBToHSV(activePlayerPreference.PreferredColor(), out float hue, out float saturation, out float brightness); // HSV => Hue Saturation Value, HSB => Hue Saturation Brightness
                MainMenuJoinServerPanel.Instance.UpdatePanelValues(serverIp, activePlayerPreference.PlayerName, new Vector3(hue, saturation, brightness));

                if (multiplayerSession.SessionPolicy.RequiresServerPassword)
                {
                    Log.Info("Waiting for server password input");
                    Log.InGame(Language.main.Get("Nitrox_WaitingPassword"));
                    MainMenuEnterPasswordPanel.ResetLastEnteredPassword();
                    MainMenuRightSide.main.OpenGroup(MainMenuEnterPasswordPanel.NAME);
                    break;
                }

                Log.Info("Waiting for user input");
                Log.InGame(Language.main.Get("Nitrox_WaitingUserInput"));
                MainMenuRightSide.main.OpenGroup(MainMenuJoinServerPanel.NAME);
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

    // TODO: Maybe move to MainMenuServerListPanel OnEnabled
    /// <param name="error"> If set to something it will display the message with <see cref="MainMenuNotificationPanel"/> after stopping the client</param>
    public void StopMultiplayerClient(string error = null)
    {
        if (!multiplayerClient || !Multiplayer.Main)
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

        if (error != null)
        {
            MainMenuNotificationPanel.ShowMessage($"Connection broke\n+{error}", MainMenuServerListPanel.NAME);
        }
    }
}
