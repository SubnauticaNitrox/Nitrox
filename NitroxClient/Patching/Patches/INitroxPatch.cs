using HarmonyLib;

namespace NitroxClient.Patching.Patches
{
    public interface INitroxPatch
    {
        void Patch(Harmony instance);
        void Restore(Harmony instance);
    }
}
