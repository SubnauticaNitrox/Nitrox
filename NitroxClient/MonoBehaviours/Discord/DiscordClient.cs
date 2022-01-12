using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.Helpers.DiscordGameSDK;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel;
using NitroxModel.Core;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Discord;

public class DiscordClient : MonoBehaviour
{
    private const long CLIENT_ID = 405122994348752896;

    private static DiscordClient main;
    private static Helpers.DiscordGameSDK.Discord discord;
    private static ActivityManager activityManager;
    private static Activity activity;
    private static bool showingWindow;

    private void OnEnable()
    {
        if (main)
        {
            Log.Error($"Tried to instantiate a second {nameof(DiscordClient)}");
            return;
        }

        main = this;
        DontDestroyOnLoad(gameObject);

        discord = new Helpers.DiscordGameSDK.Discord(CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
        discord.SetLogHook(Helpers.DiscordGameSDK.LogLevel.Debug, (level, message) => Log.Write((NitroxModel.Logger.LogLevel)level, "[Discord] " + message));
        activityManager = discord.GetActivityManager();
        activityManager.RegisterSteam((uint)GameInfo.Subnautica.SteamAppId);
        activityManager.OnActivityJoinRequest += ActivityJoinRequest;
        activityManager.OnActivityJoin += ActivityJoin;

        activity = new Activity();
    }

    private void OnDisable()
    {
        Log.Info("[Discord] Shutdown");
        discord.Dispose();
    }

    private void Update()
    {
        discord?.RunCallbacks();
    }

    private void ActivityJoin(string secret)
    {
        Log.Info("[Discord] Joining Server");
        if (SceneManager.GetActiveScene().name != "StartScreen" || !MainMenuMultiplayerPanel.Main)
        {
            Log.InGame("Please press on the \"Multiplayer\" in the MainMenu if you want to join a session.");
            Log.Warn("[Discord] Can't join a server outside of the main-menu.");
            return;
        }

        string[] splitSecret = secret.Split(':');
        string ip = string.Join(":", splitSecret.Take(splitSecret.Length - 1));
        string port = splitSecret.Last();
        MainMenuMultiplayerPanel.OpenJoinServerMenu(ip, port);
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
            Log.Debug("[Discord] Request window is already active.");
        }
    }

    public static void InitializeRPMenu()
    {
        activity.State = "In menu";
        activity.Assets.LargeImage = "icon";
        UpdateActivity();
    }

    public static void InitializeRPInGame(string username, int playerCount, int maxConnections)
    {
        activity.State = "In game";
        activity.Details = "Playing as " + username;
        activity.Timestamps.Start = 0;
        activity.Party.Size.CurrentSize = playerCount;
        activity.Party.Size.MaxSize = maxConnections;
        UpdateActivity();

        NitroxServiceLocator.LocateService<IPacketSender>().Send(new DiscordRequestIP(string.Empty));
    }

    public static void UpdateIpPort(string ipPort)
    {
        activity.Party.Id = "NitroxPartyID:" + ipPort;
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
        activityManager.UpdateActivity(activity, (result) =>
        {
            if (result != Result.Ok)
            {
                Log.Error("[Discord] Updating Activity failed");
            }
        });
    }

    public static void RespondJoinRequest(long userID, ActivityJoinRequestReply reply)
    {
        Log.Info($"[Discord] Responded with {reply} to JoinRequest: {userID}");
        showingWindow = false;
        activityManager.SendRequestReply(userID, reply, _ => { });
    }
}
