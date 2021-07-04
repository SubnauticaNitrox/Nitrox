using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class QuickSlots_Bind_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(QuickSlots).GetMethod(nameof(QuickSlots.Bind), BindingFlags.Public | BindingFlags.Instance);
        private static LocalPlayer player;

        public static void Postfix(QuickSlots __instance)
        {
            // TODO: The binding should only be send on a timer and/or on disconnect. But this functionality/framework is not implemented yet.
            string[] binding = __instance.SaveBinding();

            for (int i = 0; i < binding.Length; i++)
            {
                binding[i] ??= "null"; // ProtoBuf can't handle null objects in Lists
            }

            player.BroadcastQuickSlotsBindingChanged(binding.ToList());
        }

        public override void Patch(Harmony harmony)
        {
            player = NitroxServiceLocator.LocateService<LocalPlayer>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
