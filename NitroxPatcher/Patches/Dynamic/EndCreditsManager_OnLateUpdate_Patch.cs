using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EndCreditsManager_OnLateUpdate_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((EndCreditsManager t) => t.OnLateUpdate());

    public static bool EndCreditsTriggered;

    /*
     * AddressablesUtility.LoadScene("Cleaner", LoadSceneMode.Single);
     * EndCreditsManager_OnLateUpdate_Patch.OnCreditsEnd();   <--- [INSERTED LINE]
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch(OpCodes.Ldstr, "Cleaner"),
                                                new CodeMatch(OpCodes.Ldc_I4_0),
                                                new CodeMatch(OpCodes.Call)
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => OnCreditsEnd()))
                                            ]).InstructionEnumeration();
    }

    private static void OnCreditsEnd()
    {
        EndCreditsTriggered = true;
        JoinServerBackend.StopMultiplayerClient();
    }
}
