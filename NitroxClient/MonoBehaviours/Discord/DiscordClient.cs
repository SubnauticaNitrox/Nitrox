using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NitroxClient.Helpers.DiscordGameSDK;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.DiscordRP;

public class DiscordClient : MonoBehaviour
{
    private const long CLIENT_ID = 405122994348752896;

    private static Helpers.DiscordGameSDK.Discord discord;
    private static ActivityManager activityManager;
    private static Activity activity;
    private static bool showingWindow;

    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        discord = new Helpers.DiscordGameSDK.Discord(CLIENT_ID, 0);
        activityManager = discord.GetActivityManager();
        activityManager.RegisterSteam((uint)GameInfo.Subnautica.SteamAppId);
        activityManager.OnActivityJoinRequest += ActivityJoinRequest;
        activityManager.OnActivityJoin += ActivityJoin;
        discord.SetLogHook(Helpers.DiscordGameSDK.LogLevel.Debug, (level, message) => Log.Write((NitroxModel.Logger.LogLevel)level, "[Discord] " + message));

        activity = new Activity();
    }

    private void OnDisable()
    {
        Log.Info("[Discord] Shutdown");
        discord.Dispose();
    }

    private void Update()
    {
        discord.RunCallbacks();
    }

    private static void ActivityJoin(string secret)
    {
        Log.Info("[Discord] Joining Server");
        if (SceneManager.GetActiveScene().name == "StartScreen" && MainMenuMultiplayerPanel.Main)
        {
            string[] splitSecret = secret.Split(':');
            string ip = string.Join(":", splitSecret.Take(splitSecret.Length - 1));
            string port = splitSecret.Last();
            MainMenuMultiplayerPanel.OpenJoinServerMenu(ip, port);
        }
        else
        {
            Log.InGame("Please press on the \"Multiplayer\" in the MainMenu if you want to join a session.");
            Log.Warn("[Discord] Can't join a server outside of the main-menu.");
        }
    }

    private void ActivityJoinRequest(ref User user)
    {
        if (!showingWindow)
        {
            Log.Info($"[Discord] JoinRequest: Name:{user.Username}#{user.Discriminator} UserID:{user.Id}");
            DiscordJoinRequestGui acceptRequest = gameObject.AddComponent<DiscordJoinRequestGui>();
            acceptRequest.User = user;
            showingWindow = true;
        }
        else
        {
            Log.Debug("[Discord] Request window is already active.");
        }
    }

    public static void InitializeRPInGame(string username, int playerCount, int maxConnections, string ip, int port)
    {
        activity.State = "In game";
        activity.Details = "Playing as " + username;
        activity.Timestamps.Start = 0;
        activity.Party.Size.CurrentSize = playerCount;
        activity.Party.Size.MaxSize = maxConnections;

        Task<string> ipPort = CheckIP(ip, port);
        ipPort.RunSynchronously();
        activity.Party.Id = $"NitroxPartyID:{ipPort.Result}";
        activity.Secrets.Join = ipPort.Result;

        UpdateActivity();
    }

    public static void InitializeRPMenu()
    {
        activity.State = "In menu";
        activity.Assets.LargeImage = "icon";
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

    private static async Task<string> CheckIP(string ip, int port)
    {
        if (ip == IPAddress.Loopback.ToString())
        {
            Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
            if (await wanIp != null)
            {
                Log.Error("[DiscordClient] Couldn't get WAN-IP. Discord Rich Present join feature will not work.");
                return $"{IPAddress.Loopback}:{port}";
            }

            return $"{wanIp.Result}:{port}";
        }

        return $"{ip}:{port}";
    }
}
