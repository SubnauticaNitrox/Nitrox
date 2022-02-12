using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class QuickSlots_DeselectInternal_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((QuickSlots t) => t.DeselectInternal());
        private static LocalPlayer player;

        public static void Prefix(InventoryItem ____heldItem)
        {
            if (____heldItem != null && ____heldItem.item) //____heldItem.item can be null on game quitting
            {
                Pickupable pickupable = ____heldItem.item;
                NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
                PlayerTool component = pickupable.GetComponent<PlayerTool>();
                PlayerHeldItemChanged.ChangeType type = component ? PlayerHeldItemChanged.ChangeType.HOLSTER_AS_TOOL : PlayerHeldItemChanged.ChangeType.HOLSTER_AS_ITEM;
                player.BroadcastHeldItemChanged(itemId, type, null);
            }
        }

        public override void Patch(Harmony harmony)
        {
            player = NitroxServiceLocator.LocateService<LocalPlayer>();
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
