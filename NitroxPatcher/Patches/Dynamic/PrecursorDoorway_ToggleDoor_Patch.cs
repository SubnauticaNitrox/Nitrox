using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    class PrecursorDoorway_ToggleDoor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(PrecursorDoorway).GetMethod(nameof(PrecursorDoorway.ToggleDoor), BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(PrecursorDoorway __instance, bool open)
        {
            NitroxEntity entity = __instance.GetComponent<NitroxEntity>();
            NitroxServiceLocator.LocateService<PrecursorManager>().TogglePrecursorDoor(entity.Id, open);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
