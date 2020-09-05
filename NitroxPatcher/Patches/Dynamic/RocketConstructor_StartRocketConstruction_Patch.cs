using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class RocketConstructor_StartRocketConstruction_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(RocketConstructor).GetMethod("StartRocketConstruction", BindingFlags.Public | BindingFlags.Instance);

        public static readonly OpCode INJECTION_CODE = OpCodes.Stloc_2;

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                /* if (this.crafterLogic.Craft(currentStageTech, craftTime)) {
			     *      GameObject toBuild = this.rocket.StartRocketConstruction();
                 *  ->  RocketConstructor_StartRocketConstruction_Patch.Callback(this.rocket, currentStageTech, toBuild); 
			     *      ItemGoalTracker.OnConstruct(currentStageTech);
			     *      this.SendBuildBots(toBuild);
		         * }
                 */
                if (instruction.opcode == INJECTION_CODE)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(RocketConstructor).GetField("rocket", BindingFlags.Public | BindingFlags.Instance)); //this.rocket
                    yield return new CodeInstruction(OpCodes.Ldloc_0); //techtype
                    yield return new CodeInstruction(OpCodes.Ldloc_2); //toBuild GO
                    yield return new CodeInstruction(OpCodes.Call, typeof(RocketConstructor_StartRocketConstruction_Patch).GetMethod("Callback", BindingFlags.Static | BindingFlags.Public));
                }

            }
        }

        public static void Callback(Rocket rocketAttached, TechType currentStageTech, GameObject gameObjectToBuild)
        {
            NitroxId rocketId = NitroxEntity.GetId(rocketAttached.gameObject);
            NitroxServiceLocator.LocateService<Rockets>().BroadcastRocketStateUpdate(rocketId, currentStageTech, gameObjectToBuild);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
