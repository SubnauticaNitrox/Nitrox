using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAEncyclopedia_Add_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDAEncyclopedia);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Prefix(string key)
        {
            NitroxServiceLocator.LocateService<PDAEncyclopediaEntry>().Add(key);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
