using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubConsoleCommand_OnConsoleCommand_sub_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubConsoleCommand t) => t.OnConsoleCommand_sub(default));

    /*
     * REPLACE:
     * LightmappedPrefabs.main.RequestScenePrefab(text, new LightmappedPrefabs.OnPrefabLoaded(this.OnSubPrefabLoaded));
     * BY:
     * LightmappedPrefabs.main.RequestScenePrefab(text, new LightmappedPrefabs.OnPrefabLoaded(SubConsoleCommand_OnConsoleCommand_sub_Patch.WrappedCallback));
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Stfld),
                                                new CodeMatch(OpCodes.Ldsfld),
                                                new CodeMatch(OpCodes.Ldloc_0),
                                                new CodeMatch(OpCodes.Ldarg_0)
                                            ])
                                            .SetOpcodeAndAdvance(OpCodes.Ldnull)
                                            .SetOperandAndAdvance(Reflect.Method(() => WrappedCallback(default)))
                                            .InstructionEnumeration();
    }

    public static void WrappedCallback(GameObject prefab)
    {
        SubConsoleCommand instance = SubConsoleCommand.main;
        // Call the original callback and then get the object it created to broadcast its creation
        instance.OnSubPrefabLoaded(prefab);
        Resolve<NitroxConsole>().Spawn(instance.lastCreatedSub);
    }
}
