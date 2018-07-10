﻿using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class KnownTech_NotifyAdd_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(KnownTech);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyAdd", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Prefix(TechType techType, bool verbose)
        {
            NitroxServiceLocator.LocateService<KnownTechEntry>().Add(techType, verbose);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
