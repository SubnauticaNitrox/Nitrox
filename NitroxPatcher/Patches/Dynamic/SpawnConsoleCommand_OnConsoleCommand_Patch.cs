using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents local player from using "spawn" command without at least the <see cref="Perms.MODERATOR"/> permissions.
/// </summary>
public sealed partial class SpawnConsoleCommand_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnConsoleCommand t) => t.OnConsoleCommand_spawn(default));

    public static bool Prefix(NotificationCenter.Notification n)
    {
        if (Resolve<LocalPlayer>().Permissions < Perms.MODERATOR)
        {
            Log.InGame(Language.main.Get("Nitrox_MissingPermission").Replace("{PERMISSION}", Perms.MODERATOR.ToString()));
            return false;
        }
        return true;
    }
}
