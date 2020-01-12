using Harmony;

namespace NitroxPatcher.Patches
{
    public interface INitroxPatch
    {
        void Patch(HarmonyInstance instance);
        void Restore(HarmonyInstance instance);
    }
}
