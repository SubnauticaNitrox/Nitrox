using HarmonyLib;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public interface INitroxPatch
    {
        void Patch(Harmony instance);
        void Restore(Harmony instance);
    }
}
