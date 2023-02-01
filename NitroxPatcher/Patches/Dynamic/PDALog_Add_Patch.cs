using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class PDALog_Add_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDALog.Add(default, default));
    public static List<string> IgnoreKeys = new();

    public static void Postfix(string key, PDALog.EntryData __result)
    {
        if (IgnoreKeys.RemoveAll(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) > 0 || __result == null ||
            !PDALog.entries.TryGetValue(key, out PDALog.Entry entry))
        {
            return;
        }
        Resolve<IPacketSender>().Send(new PDALogEntryAdd(key, entry.timestamp));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
