using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAEncyclopedia_Add_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(PDAEncyclopedia).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static);

#if SUBNAUTICA
        public static void Prefix(string key)
        {
            NitroxServiceLocator.LocateService<PDAEncyclopediaEntry>().Add(key);
#elif BELOWZERO
        public static void Prefix(string key, bool postNotification)
        {
            NitroxServiceLocator.LocateService<PDAEncyclopediaEntry>().Add(key, postNotification);
#endif
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
