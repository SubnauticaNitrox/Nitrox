using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_Scan_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAScanner.Scan());

        private static readonly OpCode INJECTION_OPCODE = OpCodes.Call;
        private static readonly object INJECTION_OPERAND = Reflect.Method(() => ResourceTracker.UpdateFragments());

        public static readonly OpCode INJECTION_OPCODE_2 = OpCodes.Ldsfld;
        public static readonly object INJECTION_OPERAND_2 = Reflect.Field(() => PDAScanner.cachedProgress);
        
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
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => Callback()));
                }
                else if (instruction.opcode.Equals(INJECTION_OPCODE_2) && instruction.operand.Equals(INJECTION_OPERAND_2))
                {
                    yield return new CodeInstruction(OpCodes.Call, Reflect.Method(() => ProgressCallback()));
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

        // Both in milliseconds
        public const int PACKET_SENDING_RATE = 500;
        public const int LAST_PACKET_SEND_DELAY = 2000;

        public static readonly PDAManagerEntry PDAManagerEntry = NitroxServiceLocator.LocateService<PDAManagerEntry>();
        public static Dictionary<NitroxId, ThrottledEntry> ThrottlingEntries = new Dictionary<NitroxId, ThrottledEntry>();

        public class ThrottledEntry
        {
            public TechType EntryTechType;
            public NitroxId Id;
            public DateTimeOffset LatestProgressTime, LatestPacketSendTime;
            public PDAScanner.Entry Entry;
            public Coroutine LastProgressThrottler;
            public bool Unlocked;

            public ThrottledEntry(TechType techType, NitroxId id, PDAScanner.Entry entry) : this(techType, id, DateTimeOffset.FromUnixTimeMilliseconds(0), DateTimeOffset.FromUnixTimeMilliseconds(0), entry)
            { }

            public ThrottledEntry(TechType techType, NitroxId id, DateTimeOffset latestProgressTime, DateTimeOffset latestPacketSendTime, PDAScanner.Entry entry)
            {
                EntryTechType = techType;
                Id = id;
                LatestProgressTime = latestProgressTime;
                LatestPacketSendTime = latestPacketSendTime;
                Entry = entry;
            }

            public void Update(DateTimeOffset? newLatestProgressTime, DateTimeOffset? newLatestPacketSendTime, float progress)
            {
                if (newLatestProgressTime.HasValue)
                {
                    LatestProgressTime = newLatestProgressTime.Value;
                }
                if (newLatestPacketSendTime.HasValue)
                {
                    LatestPacketSendTime = newLatestPacketSendTime.Value;
                }
                Entry.progress = progress;
            }

            public void StartThrottler()
            {
                LastProgressThrottler = Player.main.StartCoroutine(ThrottleLastProgress(this));
            }

            public PDAScanner.Entry GetEntry()
            {
                if (PDAScanner.GetPartialEntryByKey(EntryTechType, out PDAScanner.Entry entry) && entry.unlocked > Entry.unlocked)
                {
                    Unlocked = true;
                    Entry.unlocked = entry.unlocked;
                }
                
                return Entry;
            }
        }

        // After 2 seconds without scanning, a packet will be sent with the latest progress
        static IEnumerator ThrottleLastProgress(ThrottledEntry throttledEntry)
        {
            do
            {
                yield return new WaitForSeconds(LAST_PACKET_SEND_DELAY / 1000);
            }
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < throttledEntry.LatestProgressTime.ToUnixTimeMilliseconds() + LAST_PACKET_SEND_DELAY);
            
            throttledEntry.GetEntry();
            if (!throttledEntry.Unlocked && !PDAScanner.ContainsCompleteEntry(throttledEntry.EntryTechType))
            {
                PDAManagerEntry.Progress(throttledEntry.Entry, throttledEntry.Id);
            }

            // No need to keep this in memory
            ThrottlingEntries.Remove(throttledEntry.Id);
        }

        // Happens each time a progress is made
        public static void ProgressCallback()
        {
            PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
            if (scanTarget.techType == TechType.Player)
            {
                return;
            }

            if (scanTarget.isValid && scanTarget.progress > 0f && scanTarget.gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
            {
                // Should always be the same for the same entity
                NitroxId nitroxId = nitroxEntity.Id;
                DateTimeOffset Now = DateTimeOffset.UtcNow;

                // Start a thread when a scanning period starts
                if (!ThrottlingEntries.TryGetValue(nitroxId, out ThrottledEntry throttledEntry))
                {
                    ThrottlingEntries.Add(nitroxId, throttledEntry = new ThrottledEntry(scanTarget.techType, nitroxId, new PDAScanner.Entry() { techType = scanTarget.techType, progress = scanTarget.progress }));

                    throttledEntry.StartThrottler();
                }
                PDAScanner.Entry scannerEntry = throttledEntry.GetEntry();

                // Updating the progress throttling
                throttledEntry.Update(Now, null, scanTarget.progress);
                // Throttling the packet sending
                if (Now.ToUnixTimeMilliseconds() < throttledEntry.LatestPacketSendTime.ToUnixTimeMilliseconds() + PACKET_SENDING_RATE)
                {
                    return;
                }

                NitroxTechType nitroxTechType = new NitroxTechType(scanTarget.techType.ToString());
                // This should not occur all the time because it only takes effect one time per entity
                // Then it occurs when someone else already scanned the entity
                // Check if the entry is already in the partial list, and then, change the progress
                if (PDAManagerEntry.CachedEntries.TryGetValue(nitroxTechType, out PDAProgressEntry pdaProgressEntry))
                {
                    if (pdaProgressEntry.Entries.TryGetValue(nitroxId, out float progress))
                    {
                        pdaProgressEntry.Entries.Remove(nitroxId);
                        // We'd like the progress to not decrease when two people scan the same thing at the same time
                        if (scannerEntry.progress < progress)
                        {
                            PDAScanner.scanTarget.progress = progress;
                            scannerEntry.progress = progress;
                        }
                        if (pdaProgressEntry.Entries.Count == 0)
                        {
                            PDAManagerEntry.CachedEntries.Remove(nitroxTechType);
                        }
                    }
                }

                throttledEntry.Update(Now, Now, scanTarget.progress);
                PDAManagerEntry.Progress(throttledEntry.GetEntry(), nitroxId);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}
