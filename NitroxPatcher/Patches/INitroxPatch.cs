using HarmonyLib;

namespace NitroxPatcher.Patches
{
    public interface INitroxPatch
    {
        void Patch(Harmony instance);
        void Restore(Harmony instance);
    }
}
