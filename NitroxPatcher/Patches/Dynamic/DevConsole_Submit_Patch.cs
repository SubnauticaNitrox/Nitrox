using System.Reflection;
using Nitrox.Model.DataStructures.GameLogic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables the DevConsole's submit functionality for players with insufficient permissions.
/// TODO: Implement https://github.com/SubnauticaNitrox/Nitrox/issues/1689
/// </summary>
public sealed partial class DevConsole_Submit_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((DevConsole t) => t.Submit(default));

    public static bool Prefix(string value)
    {
        Log.Info($"Used cheat command : '{value}'");
        Resolve<IPacketSender>().Send(new CheatCommand(value));

        // Allow submit if player has sufficient permissions
        if (Resolve<LocalPlayer>().Permissions >= Perms.MODERATOR)
        {
            return true; 
        }

        Log.InGame(Language.main.Get("Nitrox_MissingPermission").Replace("{PERMISSION}", nameof(Perms.MODERATOR)));
        return false;
    }
}
