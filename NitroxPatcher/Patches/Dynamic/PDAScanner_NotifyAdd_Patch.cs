using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAScanner.NotifyAdd(default(PDAScanner.Entry)));

        public static void Prefix(PDAScanner.Entry entry)
        {
            if (entry != null)
            {
                SendPacket<PDAEntryAdd>(new (entry.techType.ToDto(), entry.progress, entry.unlocked));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
