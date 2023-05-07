#if SUBNAUTICA
using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel.Server;

namespace NitroxPatcher.Patches.Dynamic;

/// <inheritdoc cref="CrashedShipExploder_OnConsoleCommand_Patch"/>
public sealed class GameModeConsoleCommands_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD_SURVIVAL = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_survival());
    public static readonly MethodInfo TARGET_METHOD_CREATIVE = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_creative());
    public static readonly MethodInfo TARGET_METHOD_FREEDOM = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_freedom());
    public static readonly MethodInfo TARGET_METHOD_HARDCORE = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_hardcore());

    public static bool PrefixSurvival() => BroadcastGameModeChange(NitroxGameMode.SURVIVAL);
    public static bool PrefixCreative() => BroadcastGameModeChange(NitroxGameMode.CREATIVE);
    public static bool PrefixFreedom() => BroadcastGameModeChange(NitroxGameMode.FREEDOM);
    public static bool PrefixHardcore() => BroadcastGameModeChange(NitroxGameMode.HARDCORE);

    private static bool BroadcastGameModeChange(NitroxGameMode gameMode)
    {
        Resolve<IPacketSender>().Send(new ServerCommand($"gamemode {gameMode}"));
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_SURVIVAL, ((Func<bool>)PrefixSurvival).Method);
        PatchPrefix(harmony, TARGET_METHOD_CREATIVE, ((Func<bool>)PrefixCreative).Method);
        PatchPrefix(harmony, TARGET_METHOD_FREEDOM, ((Func<bool>)PrefixFreedom).Method);
        PatchPrefix(harmony, TARGET_METHOD_HARDCORE, ((Func<bool>)PrefixHardcore).Method);
    }
}
#endif
