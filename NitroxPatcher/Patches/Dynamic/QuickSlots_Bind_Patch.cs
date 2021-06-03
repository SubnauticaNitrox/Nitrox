using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    //TODO: The binding should only be send on a timer and/or on disconnect. But this functionality is not implemented yet.
    public class QuickSlots_Bind_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(QuickSlots).GetMethod(nameof(QuickSlots.Bind), BindingFlags.Public | BindingFlags.Instance);
        private static LocalPlayer player;

        public static void Postfix(QuickSlots __instance)
        {
            player.BroadcastQuickSlotsBindingChanged(__instance.SaveBinding());
        }

        public override void Patch(Harmony harmony)
        {
            player = NitroxServiceLocator.LocateService<LocalPlayer>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
