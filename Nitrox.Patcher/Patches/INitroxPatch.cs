using Harmony;

namespace Nitrox.Patcher.Patches
{
    public interface INitroxPatch
    {
        void Patch(HarmonyInstance instance);
        void Restore(HarmonyInstance instance);
    }
}
