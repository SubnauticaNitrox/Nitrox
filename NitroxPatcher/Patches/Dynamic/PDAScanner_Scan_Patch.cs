using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcast scan progress and entries unlocking when they happen
/// </summary>
/// <remarks>
/// This is the only method that needs to be tracked to sync and persist PDAScanner's data, because the other methods
/// are either unused (found out that they have no actual calls in SN's code, just as )
/// </remarks>
public class PDAScanner_Scan_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAScanner.Scan());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
    internal static readonly object INJECTION_OPERAND = Reflect.Method((GameObject t) => t.SendMessage(default, default, default));

    internal static readonly OpCode INJECTION_OPCODE_2 = OpCodes.Ldsfld;
    internal static readonly object INJECTION_OPERAND_2 = Reflect.Field(() => PDAScanner.cachedProgress);

    private static readonly Dictionary<string, ThrottledEntry> throttledProgressEntries = new();

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                yield return original.Ldloc<PDAScanner.Result>();
                yield return new(OpCodes.Ldloc_S, original.GetLocalVariableIndex<bool>(4));
                yield return new(OpCodes.Call, Reflect.Method(() => Callback(default, default)));
            }
            else if (instruction.opcode.Equals(INJECTION_OPCODE_2) && instruction.operand.Equals(INJECTION_OPERAND_2))
            {
                yield return new(OpCodes.Call, Reflect.Method(() => ProgressCallback()));
            }
        }
    }

    // To determine which exact path the code took, we just need those two parameters
    public static void Callback(PDAScanner.Result result, bool alreadyComplete)
    {
        PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
        if (scanTarget.hasUID)
        {
            throttledProgressEntries.Remove(scanTarget.uid);
        }

        TechType techType = scanTarget.techType;
        PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);
        if (entryData == null || (result != PDAScanner.Result.Done && result != PDAScanner.Result.Researched))
        {
            return;
        }

        // In the case the player has already fully researched the target
        if (alreadyComplete)
        {
            // In the case that the target is destroyable, the player is given 2 titanium and the object is destroyed
            // We want to broadcast the destruction event before the object is destroyed and corresponding scan data is invalidated.
            if (scanTarget.hasUID)
            {
                Resolve<IPacketSender>().Send(new PDAScanProgress(new(scanTarget.uid), techType.ToDto(), 1f, result == PDAScanner.Result.Done));
            }
            return;
        }
        else
        {
            NitroxId targetId = scanTarget.hasUID ? new(scanTarget.uid) : null;

            // Case in which the unlocked progress is incremented
            if (result == PDAScanner.Result.Done)
            {
                if (!PDAScanner.GetPartialEntryByKey(techType, out PDAScanner.Entry entry))
                {
                    // Something wrong happened
                    return;
                }
                PDAScanFinished packet = new(targetId, techType.ToDto(), entry.unlocked, false, entryData.destroyAfterScan);
                Resolve<IPacketSender>().Send(packet);
            }
            // Case in which the scanned entry is fully unlocked
            else if (result == PDAScanner.Result.Researched)
            {
                PDAScanFinished packet = new(targetId, techType.ToDto(), entryData.totalFragments, true, entryData.destroyAfterScan);
                Resolve<IPacketSender>().Send(packet);
            }
        }
    }
    
    // Both in milliseconds
    private const int PACKET_SENDING_RATE = 500;

    public class ThrottledEntry
    {
        public NitroxTechType EntryTechType;
        public NitroxId EntityId;
        public DateTimeOffset LatestPacketSendTime, LastBroadcastTime;
        public float Progress, LastBroadcastProgress;

        public ThrottledEntry(TechType entryTechType, NitroxId entityId)
        {
            EntryTechType = entryTechType.ToDto();
            EntityId = entityId;
        }

        /// <summary>
        /// Broadcasts the latest packet if necessary
        /// </summary>
        /// <returns>True if the update</returns>
        public bool Update(DateTimeOffset now, float throttleTime)
        {
            // We'll just remove this entry if it was fully scanned
            if (!PDAScanner.cachedProgress.ContainsKey($"{EntityId}"))
            {
                return true;
            }
            if ((now - LastBroadcastTime).TotalMilliseconds >= throttleTime)
            {
                if (LastBroadcastProgress == Progress)
                {
                    return false;
                }
                else
                {
                    LastBroadcastProgress = Progress;
                    LastBroadcastTime = now;
                    Resolve<IPacketSender>().Send(new PDAScanProgress(EntityId, EntryTechType, Progress, false));
                }
            }
            return true;
        }
    }

    private static void BroadcastThrottledProgress()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<string> finishedThrottledEntryIds = new();
        foreach (KeyValuePair<string, ThrottledEntry> entry in throttledProgressEntries)
        {
            if (!entry.Value.Update(now, PACKET_SENDING_RATE))
            {
                finishedThrottledEntryIds.Add(entry.Key);
            }
        }
        finishedThrottledEntryIds.ForEach(id => throttledProgressEntries.Remove(id));
    }

    // Happens each time a progress is made when scanning an entity
    public static void ProgressCallback()
    {
        PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
        // We only need to manage unfinished progress of entities that actually have a NitroxEntity (special protection)
        if (scanTarget.isPlayer || scanTarget.progress >= 1f || scanTarget.progress == 0 || !scanTarget.isValid ||
            !scanTarget.gameObject.GetComponent<NitroxEntity>())
        {
            return;
        }

        if (!throttledProgressEntries.TryGetValue(scanTarget.uid, out ThrottledEntry throttledProgressEntry))
        {
            throttledProgressEntry = throttledProgressEntries[scanTarget.uid] = new(scanTarget.techType, new(scanTarget.uid));        
        }
        throttledProgressEntry.Progress = scanTarget.progress;
    }

    public override void Patch(Harmony harmony)
    {
        // If we patch multiple times, we'll want to make sure that we haven't subscribed multiple times to this event
        ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.Update, BroadcastThrottledProgress);
        ManagedUpdate.Subscribe(ManagedUpdate.Queue.Update, BroadcastThrottledProgress);
        PatchTranspiler(harmony, TARGET_METHOD);
    }
}
