using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PDALog_Add_Patch : NitroxPatch, IDynamicPatch
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
}
