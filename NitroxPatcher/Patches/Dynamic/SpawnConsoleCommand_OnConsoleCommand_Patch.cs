using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SpawnConsoleCommand_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((SpawnConsoleCommand t) => t.SpawnAsync(default)));

    /*
     * GameObject gameObject = global::Utils.CreatePrefab(prefabForTechType, maxDist, i > 0);
     * -> SpawnConsoleCommand_OnConsoleCommand_Patch.Callback(gameObject);
     * LargeWorldEntity.Register(gameObject);
     * CrafterLogic.NotifyCraftEnd(gameObject, techType);
     * gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch(OpCodes.Call, Reflect.Method(() => Utils.CreatePrefab(default, default, default)))
                                            ])
                                            .Advance(1)
                                            .Insert([
                                                new CodeInstruction(OpCodes.Dup),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => Callback(default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void Callback(GameObject gameObject)
    {
        Resolve<NitroxConsole>().Spawn(gameObject);
    }
}
