using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class QuickSlots_SelectInternal_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(QuickSlots).GetMethod("SelectInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        private static LocalPlayer player;

        public static void Postfix(InventoryItem ____heldItem)
        {
            Pickupable pickupable = ____heldItem.item;
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            PlayerTool component = pickupable.GetComponent<PlayerTool>();
            player.BroadcastHeldItemChanged(itemId, component ? PlayerHeldItemChangedType.DRAW_AS_TOOL : PlayerHeldItemChangedType.DRAW_AS_ITEM);
        }

        public override void Patch(Harmony harmony)
        {
            player = NitroxServiceLocator.LocateService<LocalPlayer>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
