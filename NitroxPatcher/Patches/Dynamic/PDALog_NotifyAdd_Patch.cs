using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDALog_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDALog.NotifyAdd(default(PDALog.Entry)));

        public static void Prefix(PDALog.Entry entry)
        {
            if (entry != null)
            {
                SendPacket<PDALogEntryAdd>(new (entry.data.key, entry.timestamp));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
