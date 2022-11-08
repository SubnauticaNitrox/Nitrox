using HarmonyLib;
using NitroxModel;

namespace NitroxPatcher.Patches
{
    public interface INitroxPatch : IPatch
    {
        void Patch(Harmony instance);
        void Restore(Harmony instance);
    }
}
