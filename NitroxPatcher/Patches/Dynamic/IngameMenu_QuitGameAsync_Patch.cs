using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Skips the GameModeUtils.IsPermadeath() branch so it doesn't call SaveGameAsync().
/// Inserts IMultiplayerSession.Disconnect() after LargeWorldStreamer streamer = LargeWorldStreamer.main
/// </summary>
public sealed partial class IngameMenu_QuitGameAsync_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo enumeratorMethod = Reflect.Method((IngameMenu t) => t.QuitGameAsync(default));
    private static readonly MethodInfo targetMethod = AccessTools.EnumeratorMoveNext(enumeratorMethod);

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
               .MatchEndForward(
                   new CodeMatch(OpCodes.Call, Reflect.Method(() => GameModeUtils.IsPermadeath()))
               )
               .Set(OpCodes.Ldc_I4_0, null)
               .MatchEndForward(
                   new CodeMatch(OpCodes.Ldsfld, Reflect.Field(() => LargeWorldStreamer.main))
               )
               .Advance(2)
               .Insert(
                   TranspilerHelper.LocateService<IMultiplayerSession>(),
                   new CodeInstruction(OpCodes.Callvirt, Reflect.Method((IMultiplayerSession x) => x.Disconnect()))
               )
               .InstructionEnumeration();
    }
}
