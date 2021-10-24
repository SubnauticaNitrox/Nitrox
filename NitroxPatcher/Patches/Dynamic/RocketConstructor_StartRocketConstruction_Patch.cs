using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class RocketConstructor_StartRocketConstruction_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((RocketConstructor t) => t.StartRocketConstruction());
        private static readonly OpCode INJECTION_CODE = OpCodes.Stloc_2;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                /* if (this.crafterLogic.Craft(currentStageTech, craftTime)) {
			     *      GameObject toBuild = this.rocket.StartRocketConstruction();
                 *  ->  RocketConstructor_StartRocketConstruction_Patch.Callback(this.rocket, currentStageTech); 
			     *      ItemGoalTracker.OnConstruct(currentStageTech);
			     *      this.SendBuildBots(toBuild);
		         * }
                 */
                if (instruction.opcode == INJECTION_CODE)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                    yield return new CodeInstruction(OpCodes.Ldfld, Reflect.Field((RocketConstructor t) => t.rocket)); // this.rocket
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // techtype
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Callback(default(Rocket), default(TechType))));
                }
            }
        }

        private static void Callback(Rocket rocketInstanceAttachedToConstructor, TechType currentStageTech)
        {
            NitroxId rocketId = NitroxEntity.GetId(rocketInstanceAttachedToConstructor.gameObject);
            NitroxServiceLocator.LocateService<Rockets>().BroadcastRocketStateUpdate(rocketId, currentStageTech);
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
