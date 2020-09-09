using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class RocketLaunch_OnHandClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(LaunchRocket).GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public static readonly OpCode INJECTION_CODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(LaunchRocket).GetMethod("SetLaunchStarted", BindingFlags.NonPublic | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_CODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, typeof(RocketLaunch_OnHandClick_Patch).GetMethod("Callback", BindingFlags.NonPublic | BindingFlags.Static));
                }
            }
        }

        private static void Callback(LaunchRocket launchRocket)
        {
            Rocket rocket = launchRocket.gameObject.RequireComponentInParent<Rocket>();

            NitroxId rocketId = NitroxEntity.GetId(rocket.gameObject);
            NitroxServiceLocator.LocateService<Rockets>().LaunchRocket(rocketId);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
