using System;
using System.Reflection;
using HarmonyLib;
using Nitrox.Model.DataStructures.GameLogic;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <inheritdoc cref="CrashedShipExploder_OnConsoleCommand_Patch"/>
public sealed class GameModeConsoleCommands_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD_SURVIVAL = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_survival());
    public static readonly MethodInfo TARGET_METHOD_CREATIVE = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_creative());
    public static readonly MethodInfo TARGET_METHOD_FREEDOM = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_freedom());
    public static readonly MethodInfo TARGET_METHOD_HARDCORE = Reflect.Method((GameModeConsoleCommands t) => t.OnConsoleCommand_hardcore());

    public static bool PrefixSurvival() => BroadcastGameModeChange(SubnauticaGameMode.SURVIVAL);
    public static bool PrefixCreative() => BroadcastGameModeChange(SubnauticaGameMode.CREATIVE);
    public static bool PrefixFreedom() => BroadcastGameModeChange(SubnauticaGameMode.FREEDOM);
    public static bool PrefixHardcore() => BroadcastGameModeChange(SubnauticaGameMode.HARDCORE);

    private static bool BroadcastGameModeChange(SubnauticaGameMode gameMode)
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
