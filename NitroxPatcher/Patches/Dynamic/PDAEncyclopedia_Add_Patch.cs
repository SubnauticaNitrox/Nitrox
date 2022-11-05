using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAEncyclopedia_Add_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAEncyclopedia.Add(default(string), default(bool)));

        public static void Prefix(string key)
        {
            SendPacket<PDAEncyclopediaEntryAdd>(new(key));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
