using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class QuickSlots_SelectInternal_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(QuickSlots).GetMethod("SelectInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo bindingField = typeof(QuickSlots).GetField("binding", BindingFlags.NonPublic | BindingFlags.Instance);
        private static LocalPlayer player;

        public static void Prefix(QuickSlots __instance, int slotID, ref NitroxTechType __state)
        {
            InventoryItem item = ((InventoryItem[])bindingField.GetValue(__instance))[slotID];
            if (item == null)
            {
                return;
            }
            __state = Player.main.IsToolUsed(item.item.GetTechType()) ? item.item.GetTechType().ToDto() : null;
        }

        public static void Postfix(InventoryItem ____heldItem, NitroxTechType __state)
        {
            if (____heldItem == null)
            {
                return;
            }
            Pickupable pickupable = ____heldItem.item;
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            PlayerTool component = pickupable.GetComponent<PlayerTool>();
            PlayerHeldItemChanged.ChangeType type = component ? PlayerHeldItemChanged.ChangeType.DRAW_AS_TOOL : PlayerHeldItemChanged.ChangeType.DRAW_AS_ITEM;
            player.BroadcastHeldItemChanged(itemId, type, __state);
        }

        public override void Patch(Harmony harmony)
        {
            player = NitroxServiceLocator.LocateService<LocalPlayer>();
            PatchMultiple(harmony, targetMethod, prefix:true, postfix:true);
        }
    }
}
