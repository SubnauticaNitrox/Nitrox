using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

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
        
        public const int PACKET_SENDING_RATE = 500; // in milliseconds

        public static readonly PDAManagerEntry PDAManagerEntry = NitroxServiceLocator.LocateService<PDAManagerEntry>();
        public static Dictionary<TechType, ThrottledEntry> ThrottlingEntries = new Dictionary<TechType, ThrottledEntry>();

        public class ThrottledEntry
        {
            public TechType EntryTechType;
            public DateTimeOffset LatestProgressTime, LatestPacketSendTime;
            public PDAScanner.Entry Entry;
            public Coroutine LastProgressThrottler;

            public ThrottledEntry(TechType techType, PDAScanner.Entry entry) : this(techType, DateTimeOffset.FromUnixTimeMilliseconds(0), DateTimeOffset.FromUnixTimeMilliseconds(0), entry)
            { }

            public ThrottledEntry(TechType techType, DateTimeOffset latestProgressTime, DateTimeOffset latestPacketSendTime, PDAScanner.Entry entry)
            {
                EntryTechType = techType;
                LatestProgressTime = latestProgressTime;
                LatestPacketSendTime = latestPacketSendTime;
                Entry = entry;
            }

            public void Update(DateTimeOffset? newLatestProgressTime, DateTimeOffset? newLatestPacketSendTime, PDAScanner.Entry newEntry)
            {
                if (newLatestProgressTime.HasValue)
                {
                    LatestProgressTime = newLatestProgressTime.Value;
                }
                if(newLatestPacketSendTime.HasValue)
                {
                    LatestPacketSendTime = newLatestPacketSendTime.Value;
                }
                Entry = newEntry;
            }

            public void StartThrottler()
            {
                LastProgressThrottler = Player.main.StartCoroutine(ThrottleLastProgress(EntryTechType));
            }
        }

        static IEnumerator ThrottleLastProgress(TechType techType)
        {
            do
            {
                yield return new WaitForSeconds(2);
            }
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < ThrottlingEntries[techType].LatestProgressTime.ToUnixTimeMilliseconds() + 2000);

            if (!PDAScanner.ContainsCompleteEntry(techType))
            {
                PDAManagerEntry.Progress(ThrottlingEntries[techType].Entry);
            }

            // No need to keep this in memory
            ThrottlingEntries.Remove(techType);
        }
        
        // Happens each time a progress is made
        public static void ProgressCallback()
        {
            PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;

            if (scanTarget.isValid && scanTarget.progress > 0f)
            {
                PDAScanner.Entry entry = new PDAScanner.Entry() { techType = scanTarget.techType, progress = scanTarget.progress };
                DateTimeOffset Now = DateTimeOffset.UtcNow;

                // Start a thread when a scanning period starts
                if (!ThrottlingEntries.TryGetValue(scanTarget.techType, out ThrottledEntry throttledEntry))
                {
                    ThrottlingEntries.Add(scanTarget.techType, new ThrottledEntry(scanTarget.techType, entry));

                    ThrottlingEntries[scanTarget.techType].StartThrottler();
                }

                // Check if the entry is already in the partial list, and then, change the progress
                if (PDAScanner.GetPartialEntryByKey(scanTarget.techType, out PDAScanner.Entry partialEntry) && partialEntry.progress > entry.progress)
                {
                    entry.progress = partialEntry.progress;
                    PDAScanner.scanTarget.progress = partialEntry.progress;
                }

                // Updating the progress throttling
                if (ThrottlingEntries.TryGetValue(scanTarget.techType, out throttledEntry))
                {
                    throttledEntry.Update(Now, null, entry);
                }

                // Throttling the packet sending
                if (ThrottlingEntries.TryGetValue(scanTarget.techType, out throttledEntry))
                {
                    if (Now.ToUnixTimeMilliseconds() < ThrottlingEntries[scanTarget.techType].LatestPacketSendTime.ToUnixTimeMilliseconds() + PACKET_SENDING_RATE)
                    {
                        return;
                    }
                    throttledEntry.Update(Now, Now, entry);
                }
                else
                {
                    ThrottlingEntries.Add(scanTarget.techType, new ThrottledEntry(scanTarget.techType, Now, Now, entry));
                }
                PDAManagerEntry.Progress(entry);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
