using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_NotifyRemove_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAScanner.NotifyRemove(default(PDAScanner.Entry)));

        public static void Prefix(PDAScanner.Entry entry)
        {
            if (entry != null)
            {
                SendPacket<PDAEntryRemove>(new(entry.techType.ToDto()));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
