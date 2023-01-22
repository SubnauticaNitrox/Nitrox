using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// We just want to disable all these commands on client-side and redirect them as ConsoleCommand
/// TODO: Remove this file when we'll have the command system
public class CrashedShipExploder_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_COUNTDOWNSHIP = Reflect.Method((CrashedShipExploder t) => t.OnConsoleCommand_countdownship());
    private static readonly MethodInfo TARGET_METHOD_EXPLODEFORCE = Reflect.Method((CrashedShipExploder t) => t.OnConsoleCommand_explodeforce());
    private static readonly MethodInfo TARGET_METHOD_EXPLODESHIP = Reflect.Method((CrashedShipExploder t) => t.OnConsoleCommand_explodeship());
    private static readonly MethodInfo TARGET_METHOD_RESTORESHIP = Reflect.Method((CrashedShipExploder t) => t.OnConsoleCommand_restoreship());

    public static bool PrefixCountdownShip()
    {
        Resolve<IPacketSender>().Send(new ServerCommand("aurora countdown"));
        return false;
    }

    // This command's purpose is just to show FX, we don't need to sync it
    public static bool PrefixExplodeForce()
    {
        return true;
    }

    public static bool PrefixExplodeShip()
    {
        Resolve<IPacketSender>().Send(new ServerCommand("aurora explode"));
        return false;
    }

    public static bool PrefixRestoreShip()
    {
        Resolve<IPacketSender>().Send(new ServerCommand("aurora restore"));
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_COUNTDOWNSHIP, nameof(PrefixCountdownShip));
        PatchPrefix(harmony, TARGET_METHOD_EXPLODEFORCE, nameof(PrefixExplodeForce));
        PatchPrefix(harmony, TARGET_METHOD_EXPLODESHIP, nameof(PrefixExplodeShip));
        PatchPrefix(harmony, TARGET_METHOD_RESTORESHIP, nameof(PrefixRestoreShip));
    }
}
