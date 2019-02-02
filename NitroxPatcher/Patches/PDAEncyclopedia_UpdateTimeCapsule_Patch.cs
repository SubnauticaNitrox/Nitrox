using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxPatcher.Patches
{
    public class PDAEncyclopedia_UpdateTimeCapsule_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDAEncyclopedia);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UpdateTimeCapsule", BindingFlags.Public | BindingFlags.Static);

        public static void Prefix(string key)
        {
            NitroxServiceLocator.LocateService<PDAEncyclopediaEntry>().Update(new EncyclopediaEntry(key, true));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
