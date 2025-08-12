using System;
using System.Linq;
using DiscordGameSDKWrapper;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxModel;
using NitroxModel.Core;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Discord;

public class DiscordClient : MonoBehaviour
{
    private const long CLIENT_ID = 405122994348752896;
    private const int RETRY_INTERVAL = 60;

    private static DiscordClient main;
    private static DiscordGameSDKWrapper.Discord discord;
    private static ActivityManager activityManager;
    private static Activity activity;
    private static bool showingWindow;

    private void Awake()
    {
        if (main)
        {
            Log.Error($"[Discord] Tried to instantiate a second {nameof(DiscordClient)}");
            return;
        }
        activity = new();
        main = this;
        DontDestroyOnLoad(gameObject);
        Log.Info("[Discord] Starting Discord client");
        StartDiscordHook();
    }

    private void StartDiscordHook()
    {
        try
        {
            discord = new DiscordGameSDKWrapper.Discord(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
            discord.SetLogHook(DiscordGameSDKWrapper.LogLevel.Debug, (level, message) => Log.Write((NitroxModel.Logger.LogLevel)level, $"[Discord] {message}"));
            activityManager = discord.GetActivityManager();

            activityManager.RegisterSteam((uint)GameInfo.Subnautica.SteamAppId);
            activityManager.OnActivityJoinRequest += ActivityJoinRequest;
            activityManager.OnActivityJoin += ActivityJoin;
            if (!string.IsNullOrEmpty(activity.State))
            {
                UpdateActivity();
            }
        }
        catch (Exception ex)
        {
            DisposeAndScheduleHookRestart();
            Log.ErrorOnce($"Encountered an error while starting Discord hook, will retry every {RETRY_INTERVAL} seconds: {ex.Message}");
        }
    }

    private void OnDisable()
    {
        Log.Info("[Discord] Shutdown client");
        discord?.Dispose();
    }

    private void OnDestroy()
    {
        if (main == this)
        {
            main = null;
            activity = default;
        }
    }

    private void Update()
    {
        try
        {
            discord?.RunCallbacks();
        }
        catch (Exception ex)
        {
            // Happens when Discord is closed while Nitrox has its Discord hook running (and for other reason)
            DisposeAndScheduleHookRestart();
            Log.ErrorOnce($"An error occured while running callbacks for Discord, will retry every {RETRY_INTERVAL} seconds: {ex.Message}");
        }
    }

    private void DisposeAndScheduleHookRestart()
    {
        discord?.Dispose();
        discord = null;
        Invoke(nameof(StartDiscordHook), RETRY_INTERVAL);
    }

    private void ActivityJoin(string secret)
    {
        Log.Info("[Discord] Joining Server");

        if (SceneManager.GetActiveScene().name != "StartScreen" || !MainMenuServerListPanel.Main)
        {
            Log.InGame(Language.main.Get("Nitrox_DiscordMultiplayerMenu"));
            Log.Warn("[Discord] Can't join a server outside of the main-menu.");
            return;
        }

        string[] splitSecret = secret.Split(':');
        string ip = string.Join(":", splitSecret.Take(splitSecret.Length - 1));
        string port = splitSecret.Last();

        if(int.TryParse(port, out int portInt))
        {
            Log.Error($"[Discord] Port from received secret can't be parsed as int: {port}");
            return;
        }
        MainMenuServerButton.OpenJoinServerMenuAsync(ip, portInt).ContinueWithHandleError(true);
    }

    private void ActivityJoinRequest(ref User user)
    {
        if (!showingWindow && Multiplayer.Active)
        {
            Log.Info($"[Discord] JoinRequest: Name:{user.Username}#{user.Discriminator} UserID:{user.Id}");
            StartCoroutine(DiscordJoinRequestGui.SpawnGui(user));
            showingWindow = true;
        }
        else
        {
            Log.Warn("[Discord] Request window is already active.");
        }
    }

    public static void InitializeRPMenu()
    {
        activity.State = Language.main.Get("Nitrox_DiscordMainMenuState");
        activity.Assets.LargeImage = "icon";
        UpdateActivity();
    }

    public static void InitializeRPInGame(string username, int playerCount, int maxConnections)
    {
        activity.State = Language.main.Get("Nitrox_DiscordInGameState");
        activity.Details = Language.main.Get("Nitrox_DiscordInGame").Replace("{PLAYER}", username);
        activity.Timestamps.Start = 0;
        activity.Party.Size.CurrentSize = playerCount;
        activity.Party.Size.MaxSize = maxConnections;
        UpdateActivity();

        NitroxServiceLocator.LocateService<IPacketSender>().Send(new DiscordRequestIP(string.Empty));
    }

    public static void UpdateIpPort(string ipPort)
    {
        activity.Party.Id = $"NitroxPartyID:{ipPort}";
        activity.Secrets.Join = ipPort;
        UpdateActivity();
    }

    public static void UpdatePartySize(int size)
    {
        activity.Party.Size.CurrentSize = size;
        UpdateActivity();
    }

    private static void UpdateActivity()
    {
        activityManager?.UpdateActivity(activity, (result) =>
        {
            if (result != Result.Ok)
            {
                Log.Error($"[Discord] {result}: Updating Activity failed");
            }
        });
    }

    public static void RespondJoinRequest(long userID, ActivityJoinRequestReply reply)
    {
        showingWindow = false;
        activityManager?.SendRequestReply(userID, reply, (result) =>
        {
            if (result == Result.Ok)
            {
                Log.Info($"[Discord] Responded successfully {reply} to {userID}");
            }
            else
            {
                Log.InGame($"[Discord] {Language.main.Get("Nitrox_Failure")}");
                Log.Error($"[Discord] {result}: Failed to send join response");
            }
        });
    }
}
