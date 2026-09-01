using System;
using System.Linq;
using System.Threading;
using DiscordGameSDKWrapper;
using Microsoft.Extensions.Hosting;
using Nitrox.Model;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Core;
using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using UnityEngine.SceneManagement;

namespace NitroxClient.Services.Game;

internal sealed class DiscordClientService(IPacketSender packetSender, GameInfo gameInfo) : IGameService, IMultiplayerGameService, IHostedService
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly GameInfo gameInfo = gameInfo;
    private const long CLIENT_ID = 405122994348752896;
    private const int RETRY_INTERVAL_SECONDS = 60;
    private static bool initialized;

    private static DiscordGameSDKWrapper.Discord? discord;
    private static ActivityManager? activityManager;
    private static Activity activity;
    private static bool showingWindow;

    private static bool? isDiscordRequested;
    private static bool IsDiscordRequested => isDiscordRequested ??= Array.Exists(Environment.GetCommandLineArgs(), arg => arg.Equals(DiscordConstants.ENABLE_ARG, StringComparison.OrdinalIgnoreCase));

    public static void UpdateIpPort(string ipPort)
    {
        if (!IsDiscordRequested)
        {
            return;
        }
        activity.Party.Id = $"NitroxPartyID:{ipPort}";
        activity.Secrets.Join = ipPort;
        UpdateActivity();
    }

    public static void UpdatePartySize(int size)
    {
        if (!IsDiscordRequested)
        {
            return;
        }
        activity.Party.Size.CurrentSize = size;
        UpdateActivity();
    }

    public static void RespondJoinRequest(long userID, ActivityJoinRequestReply reply)
    {
        if (!IsDiscordRequested)
        {
            return;
        }
        showingWindow = false;
        activityManager?.SendRequestReply(userID, reply, result =>
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

    public void Update()
    {
        try
        {
            discord?.RunCallbacks();
        }
        catch (Exception ex)
        {
            // Happens when Discord is closed while Nitrox has its Discord hook running (and for other reason)
            _ = DisposeAndScheduleHookRestartAsync().ContinueWithHandleError();
            Log.ErrorOnce($"An error occurred while running callbacks for Discord, will retry every {RETRY_INTERVAL_SECONDS} seconds: {ex.Message}");
        }
    }

    public void Start()
    {
    }

    public void Started()
    {
        if (!IsDiscordRequested)
        {
            return;
        }
        activity.State = Language.main.Get("Nitrox_DiscordInGameState");
        activity.Details = Language.main.Get("Nitrox_DiscordInGame").Replace("{PLAYER}", username);
        activity.Timestamps.Start = 0;
        activity.Party.Size.CurrentSize = playerCount;
        activity.Party.Size.MaxSize = maxConnections;
        UpdateActivity();

        packetSender.Send(new DiscordRequestIP(string.Empty));
    }

    public void Stop()
    {
        activity = default;
    }

    public void SceneChange(string name)
    {
        if (name == "XMenu")
        {
            if (!IsDiscordRequested)
            {
                return;
            }
            activity.State = Language.main.Get("Nitrox_DiscordMainMenuState");
            activity.Assets.LargeImage = "icon";
            UpdateActivity();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!IsDiscordRequested)
        {
            Log.Info("[Discord] Skipping initialization, not enabled by user");
            return;
        }
        if (initialized)
        {
            Log.Error($"[Discord] Tried to instantiate a second {nameof(DiscordClientService)}");
            return;
        }
        if (NitroxEnvironment.IsWine)
        {
            Log.Warn("[Discord] Unable to start RPC inside wine environment");
            return;
        }

        initialized = true;
        Log.Info("[Discord] Starting Discord client");
        await StartDiscordHookAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Info("[Discord] Shutdown client");
        discord?.Dispose();
        return Task.CompletedTask;
    }

    private static void UpdateActivity()
    {
        activityManager?.UpdateActivity(activity, result =>
        {
            if (result != Result.Ok)
            {
                Log.Error($"[Discord] {result}: Updating Activity failed");
            }
        });
    }

    private async Task StartDiscordHookAsync()
    {
        try
        {
            discord = new DiscordGameSDKWrapper.Discord(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
            discord.SetLogHook(DiscordGameSDKWrapper.LogLevel.Debug, (level, message) => Log.Write((Nitrox.Model.Logger.LogLevel)level, $"[Discord] {message}"));

            activityManager = discord.GetActivityManager();

            if (activityManager == null)
            {
                Log.Error("[Discord] Failed to get activity manager from Discord");
                return;
            }

            activityManager.RegisterSteam((uint)gameInfo.SteamAppId);
            activityManager.OnActivityJoinRequest += ActivityJoinRequest;
            activityManager.OnActivityJoin += ActivityJoin;

            if (!string.IsNullOrEmpty(activity.State))
            {
                UpdateActivity();
            }
        }
        catch (Exception ex)
        {
            await DisposeAndScheduleHookRestartAsync();
            Log.ErrorOnce($"Encountered an error while starting Discord hook, will retry every {RETRY_INTERVAL_SECONDS} seconds: {ex.Message}");
        }
    }

    private async Task DisposeAndScheduleHookRestartAsync()
    {
        discord?.Dispose();
        discord = null;
        await Task.Delay(TimeSpan.FromSeconds(RETRY_INTERVAL_SECONDS));
        await StartDiscordHookAsync();
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

        if (int.TryParse(port, out int portInt))
        {
            Log.Error($"[Discord] Port from received secret can't be parsed as int: {port}");
            return;
        }
        MainMenuServerButton.OpenJoinServerMenuAsync(ip, portInt).ContinueWithHandleError(true);
    }

    private void ActivityJoinRequest(ref User user)
    {
        if (!showingWindow && MonoBehaviours.Multiplayer.Active)
        {
            Log.Info($"[Discord] JoinRequest: Name:{user.Username}#{user.Discriminator} UserID:{user.Id}");
            StartCoroutineDetached(DiscordJoinRequestGui.SpawnGui(user));
            showingWindow = true;
        }
        else
        {
            Log.Warn("[Discord] Request window is already active.");
        }
    }
}
