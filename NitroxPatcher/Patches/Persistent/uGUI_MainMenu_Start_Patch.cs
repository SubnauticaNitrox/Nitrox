#if DEBUG
using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

// TODO: Rework this to be less ad hoc and more robust with command line arguments
public sealed partial class uGUI_MainMenu_Start_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD_ENUMERATOR = Reflect.Method((uGUI_MainMenu t) => t.Start());
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(TARGET_METHOD_ENUMERATOR);

    private static bool applied;

    public static void Postfix()
    {
        if (applied)
        {
            return;
        }
        applied = true;

        string[] args = Environment.GetCommandLineArgs();
        Log.Info(string.Join(" ", args));
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("-instantlaunch", StringComparison.OrdinalIgnoreCase) && args.Length > i + 1)
            {
                Log.Info("Detected instant launch, connecting to 127.0.0.1:11000");
                MainMenuNotificationPanel.ShowLoading();
                JoinServerBackend.InstantLaunch = true;
                JoinServerBackend.StartMultiplayerClientAsync("127.0.0.1", 11000).ContinueWithHandleError(ex =>
                {
                    Log.Error(ex);
                    Log.InGame(ex.Message);
                });
            }
        }
    }
}
#endif
