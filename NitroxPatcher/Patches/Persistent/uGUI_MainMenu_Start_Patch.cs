#if DEBUG
using System;
using System.Net;
using System.Numerics;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxModel;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;

namespace NitroxPatcher.Patches.Persistent;

// TODO: Rework this to be less ad hoc and more robust with command line arguments
public sealed partial class uGUI_MainMenu_Start_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((uGUI_MainMenu t) => t.Start()));

    private static bool applied;
    private static string playerName;

    public static void Postfix()
    {
        if (applied)
        {
            return;
        }
        applied = true;

        string[] args = Environment.GetCommandLineArgs();
        Log.Info($"CommandLineArgs: {string.Join(" ", args)}");
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--instantlaunch", StringComparison.OrdinalIgnoreCase) && args.Length > i + 1)
            {
                playerName = args[i + 1];
                Log.Info($"Detected instant launch, connecting to 127.0.0.1:11000 as {playerName}");
                _ = JoinServerBackend.StartDetachedMultiplayerClientAsync(IPAddress.Loopback, 11000, SessionConnectionStateChangedHandler);
            }
        }
    }

    private static void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
    {
        switch (state.CurrentStage)
        {
            case MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS:

                if (Resolve<IMultiplayerSession>().SessionPolicy.RequiresServerPassword)
                {
                    Log.Error("Local server requires a password which is not supported with instant launch.");
                    Log.InGame("Local server requires a password which is not supported with instant launch.");
                    break;
                }

                NitroxColor playerColor = new(1,1,1);
                byte[] nameHash = playerName.AsMd5Hash();
                if (nameHash.Length >= 8)
                {
                    float hue = BitConverter.ToUInt64([nameHash[0], nameHash[1], nameHash[2], nameHash[3], nameHash[4], nameHash[5], nameHash[6], nameHash[7]], 0) / (float)ulong.MaxValue;
                    playerColor = NitroxColor.FromHsb(hue);
                }
                PlayerSettings playerSettings = new(playerColor);
                AuthenticationContext authenticationContext = new(playerName, Optional.Empty);
                Resolve<IMultiplayerSession>().RequestSessionReservation(playerSettings, authenticationContext);
                break;

            case MultiplayerSessionConnectionStage.SESSION_RESERVED:
                Resolve<IMultiplayerSession>().ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                JoinServerBackend.StartGame();
                break;
        }
    }
}
#endif
