using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_Scan_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDAScanner);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Scan", BindingFlags.Public | BindingFlags.Static);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        public static readonly object INJECTION_OPERAND = typeof(ResourceTracker).GetMethod("UpdateFragments", BindingFlags.Public | BindingFlags.Static);

        public static readonly OpCode INJECTION_OPCODE_2 = OpCodes.Ldsfld;
        public static readonly object INJECTION_OPERAND_2 = typeof(PDAScanner).GetField("cachedProgress", BindingFlags.NonPublic | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);
            Validate.NotNull(INJECTION_OPERAND_2);

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
                else if(instruction.opcode.Equals(INJECTION_OPCODE_2) && instruction.operand.Equals(INJECTION_OPERAND_2))
                {
                    yield return new CodeInstruction(OpCodes.Call, typeof(PDAScanner_Scan_Patch).GetMethod("ProgressCallback", BindingFlags.Static | BindingFlags.Public));
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
            if (entryData != null && entryData.destroyAfterScan && PDAScanner.scanTarget.gameObject && !PDAScanner.scanTarget.isPlayer)
            {
                // A lot of fragments are virtual entities (spawned by placeholders in the world).  Sometimes the server only knows the id
                // of the placeholder and not the virtual entity. TODO: we will need to propagate deterministic ids to children entities for
                // these virtual entities.
                NitroxServiceLocator.LocateService<Item>().PickedUp(PDAScanner.scanTarget.gameObject, techType);
            }
        }

        public static readonly int PACKET_SENDING_RATE = 500; // in milliseconds

        // Tuple<LatestProgressTime, LatestPacketSendTime, Entry>
        public static Dictionary<TechType, Tuple<DateTimeOffset, DateTimeOffset, PDAScanner.Entry>> ThrottlingEntries = new Dictionary<TechType, Tuple<DateTimeOffset, DateTimeOffset, PDAScanner.Entry>>();
        // Store the thread concerning a certain type
        public static Dictionary<TechType, Thread> ThrottlingThreads = new Dictionary<TechType, Thread>();

        // After 2 seconds without scanning, a packet will be sent with the latest progress
        public static void Throttle(TechType techType)
        {
            do
            {
                Thread.Sleep(2000);
                Log.Debug($"Throttled {techType} waiting");
            }
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < ThrottlingEntries[techType].Item1.ToUnixTimeMilliseconds() + 2000);
            Log.Debug($"Throttled {techType} will now be sent");

            // Don't send the packet if the scan is already finished
            if (!PDAScanner.ContainsCompleteEntry(techType))
            {
                NitroxServiceLocator.LocateService<PDAManagerEntry>().Progress(ThrottlingEntries[techType].Item3);
            }
            
            // No need to keep these in memory
            ThrottlingEntries.Remove(techType);
            ThrottlingThreads.Remove(techType);
        }
        
        // Happens each time a progress is made
        public static void ProgressCallback()
        {
            PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;

            if (scanTarget.isValid && scanTarget.progress > 0f)
            {
                PDAScanner.Entry entry = new PDAScanner.Entry() { techType = scanTarget.techType, progress = scanTarget.progress };

                // Start a thread when a scanning period starts
                if (!ThrottlingThreads.ContainsKey(scanTarget.techType))
                {
                    ThrottlingThreads.Add(scanTarget.techType, new Thread(() => {
                        Throttle(scanTarget.techType);
                    }));
                    ThrottlingThreads[scanTarget.techType].Start();
                }


                // Check if the entry is already in the partial list, and then, change the progress
                if (PDAScanner.GetPartialEntryByKey(scanTarget.techType, out PDAScanner.Entry partialEntry) && partialEntry.progress > entry.progress)
                {
                    entry.progress = partialEntry.progress;
                    PDAScanner.scanTarget.progress = partialEntry.progress;
                }

                // Updating the progress throttling
                if (ThrottlingEntries.ContainsKey(scanTarget.techType))
                {
                    ThrottlingEntries[scanTarget.techType] = Tuple.Create(DateTimeOffset.UtcNow, ThrottlingEntries[scanTarget.techType].Item2, entry);
                }

                // Throttling the packet sending
                if (ThrottlingEntries.ContainsKey(scanTarget.techType) && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < ThrottlingEntries[scanTarget.techType].Item2.ToUnixTimeMilliseconds() + PACKET_SENDING_RATE)
                {
                    return;
                }
                ThrottlingEntries[scanTarget.techType] = Tuple.Create(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, entry);
                NitroxServiceLocator.LocateService<PDAManagerEntry>().Progress(entry);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
