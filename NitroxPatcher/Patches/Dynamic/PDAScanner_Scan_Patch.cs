using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_Scan_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDAScanner);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Scan", BindingFlags.Public | BindingFlags.Static);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(ResourceTracker).GetMethod("UpdateFragments", BindingFlags.Public | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * ResourceTracker::UpdateFragments()
                     * >> PDAScanner_Scan_Patch.Callback();
                     */
                    yield return new CodeInstruction(OpCodes.Call, typeof(PDAScanner_Scan_Patch).GetMethod("Callback", BindingFlags.Static | BindingFlags.Public));
                }
            }
        }

        public static void Callback()
        {
            // When a player scans a fragment, it will be deleted from the world. We want to send out a pickup event
            // before the object can be removed and corresponding scan data is invalidated.
            TechType techType = PDAScanner.scanTarget.techType;
            PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

            // Only do this for fragments and player scans or nearby fish
#if SUBNAUTICA
            if (entryData != null && entryData.destroyAfterScan && PDAScanner.scanTarget.gameObject && !PDAScanner.scanTarget.isPlayer)
#elif BELOWZERO
            if (entryData != null && entryData.destroyAfterScan && PDAScanner.scanTarget.gameObject)
#endif
            {
                // A lot of fragments are virtual entities (spawned by placeholders in the world).  Sometimes the server only knows the id
                // of the placeholder and not the virtual entity. TODO: we will need to propagate deterministic ids to children entities for
                // these virtual entities.
                NitroxServiceLocator.LocateService<Item>().PickedUp(PDAScanner.scanTarget.gameObject, techType);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
