using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public partial class MainMenuRightSide_OpenGroup_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MainMenuRightSide t) => t.OpenGroup(default(string)));

    public static void Prefix(string target)
    {
        // Stopping the client if we leave the joining process
        if (target is not (MainMenuJoinServerPanel.NAME or MainMenuEnterPasswordPanel.NAME or MainMenuNotificationPanel.NAME))
        {
            JoinServerBackend.StopMultiplayerClient();
        }
    }
}
