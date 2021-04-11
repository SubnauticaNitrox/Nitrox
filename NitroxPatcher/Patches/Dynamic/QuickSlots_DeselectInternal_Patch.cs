using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class QuickSlots_DeselectInternal_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(QuickSlots).GetMethod("DeselectInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        private static LocalPlayer player;

        public static void Prefix(InventoryItem ____heldItem)
        {
            if (____heldItem != null)
            {
                Pickupable pickupable = ____heldItem.item;
                NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
                PlayerTool component = pickupable.GetComponent<PlayerTool>();
                player.BroadcastHeldItemChanged(itemId, component ? PlayerHeldItemChangedType.HOLSTER_AS_TOOL : PlayerHeldItemChangedType.HOLSTER_AS_ITEM);
            }
        }

        public override void Patch(Harmony harmony)
        {
            player = NitroxServiceLocator.LocateService<LocalPlayer>();
            PatchPrefix(harmony, targetMethod);
        }
    }
}
