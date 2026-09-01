using System.Collections;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Subnautica.MultiplayerSession;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using UnityEngine;

namespace NitroxClient.GameLogic;

internal sealed class JoinServerBackend(IMultiplayerSession multiplayerSession, PlayerPreferenceManager playerPreferenceManager, IServiceScopeFactory scopeFactory)
{
    private PlayerPreferenceManager preferencesManager = playerPreferenceManager;
    private PlayerPreference? activePlayerPreference = null;
    private readonly IMultiplayerSession multiplayerSession = multiplayerSession;
    private readonly IServiceScopeFactory scopeFactory = scopeFactory;

    private IServiceScope? multiplayerScope;
    private string serverIp = "";
    private int serverPort;

    public void RequestSessionReservation(string playerName, Color playerColor)
    {
        preferencesManager.SetPreference(serverIp, new PlayerPreference(playerName, playerColor));

        Optional<string> opPassword = MainMenuEnterPasswordPanel.LastEnteredPassword;
        AuthenticationContext authenticationContext = new(playerName, opPassword);

        multiplayerSession.RequestSessionReservation(new PlayerSettings(playerColor.ToDto()), authenticationContext);
    }

    /// <summary>
    ///     This method starts a connection with the provided server but leaves handling the session negotiation for the
    ///     caller.
    /// </summary>
    public async Task StartDetachedMultiplayerClientAsync(IPAddress ip, int port, MultiplayerSessionConnectionStateChangedEventHandler sessionHandler)
    {
        Task task = StartMultiplayerClientAsync(ip, port);
        multiplayerSession.ConnectionStateChanged += sessionHandler;
        await task;
    }

    public static void StartGame()
    {
        Multiplayer.SubnauticaLoadingStarted();
#pragma warning disable CS0618 // God Damn it UWE...
        IEnumerator startNewGame = uGUI_MainMenu.main.StartNewGame(GameMode.Survival);
#pragma warning restore CS0618 // God damn it UWE...
        StartCoroutineDetached(startNewGame);
        TopRightWatermarkText.Initialize();
    }

    public async Task StartMultiplayerClientAsync(IPAddress ip, int port)
    {
        serverIp = ip.ToString();
        serverPort = port;
        activePlayerPreference = preferencesManager.GetPreference(serverIp);
        multiplayerScope = scopeFactory.CreateScope();

        multiplayerSession.ConnectionStateChanged += SessionConnectionStateChangedHandler;
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
                if (Process.GetProcessesByName("Nitrox.Server.Subnautica").Length == 0)
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

    public void StopMultiplayerClient()
    {
        if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.DISCONNECTED)
        {
            multiplayerSession.Disconnect();
        }
        multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
        multiplayerScope?.Dispose();
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
}
