using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    class Welder_Weld_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(Welder).GetMethod("Weld", BindingFlags.Instance | BindingFlags.NonPublic);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(Welder_Weld_Patch).GetMethod("AddHealthOverride", BindingFlags.Static | BindingFlags.Public);

        public static readonly OpCode SWAP_INSTRUCTION_OPCODE = OpCodes.Callvirt;
        public static readonly MethodInfo SWAP_INSTRUCTION_OPERAND = typeof(LiveMixin).GetMethod("AddHealth", BindingFlags.Public | BindingFlags.Instance);
        public static Welder RESPONSE_WELDER = null;
        public static bool Prefix()
        {
            return RESPONSE_WELDER == null;
        }
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode.Equals(SWAP_INSTRUCTION_OPCODE) && instruction.operand.Equals(SWAP_INSTRUCTION_OPERAND))
                {
                    /*
                     * Swap 
                     * this.activeWeldTarget.AddHealth(this.healthPerWeld)
                     * with
                     * AddHealthOverride(Welder welder, float addHealth)
                     */
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    yield return new CodeInstruction(INJECTION_OPCODE, INJECTION_OPERAND);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, false, true);
        }

        public static float AddHealthOverride(LiveMixin live, float addHealth, Welder welder)
        {
            Log.Debug("In AddHealthOverride");
            float result = 0f;
            if ((live.IsAlive() || live.canResurrect) && live.health < live.maxHealth)
            {
                float num = live.health;
                float newHealth = Math.Min(live.health + addHealth, live.maxHealth);
                result = newHealth - num;

                SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();
                NitroxId id = NitroxEntity.GetId(live.gameObject);
                
                // For now, we only control the LiveMixin for vehicles (not even repair nodes at a cyclops)
                // If we change that, this if should be removed!
                Vehicle vehicle = live.GetComponent<Vehicle>();
                if (vehicle)
                {
                    if (simulationOwnership.HasAnyLockType(id))
                    {
                        result = live.AddHealth(addHealth);
                    }
                    else if (simulationOwnership.OtherPlayerHasAnyLock(id))
                    {
                        // Another player simulates this entity. Send the weld info
                        Log.Debug($"Broadcast weld action for {id}");
                        NitroxServiceLocator.LocateService<LocalPlayer>().BroadcastWeld(id, addHealth);
                    }
                    else
                    {
                        // No one (presumably) simulates this entity; We try to get it
                        Log.Debug($"Try to get simulation for {id} to weld it to health!");
                        RESPONSE_WELDER = welder;
                        simulationOwnership.RequestSimulationLock(id, SimulationLockType.TRANSIENT, SimulationRequestResponse);
                        result = 0;
                    }
                }
                else
                {
                    result = live.AddHealth(addHealth);
                }
            }
            return result;
        }

        public static void SimulationRequestResponse(NitroxId id, bool lockAquired)
        {
            Welder welder = RESPONSE_WELDER;
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();            
            RESPONSE_WELDER = null;
            TARGET_METHOD.Invoke(welder, new object[] { });
        }
    }
}
