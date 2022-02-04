using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ItemsContainer_NotifyRemoveItem_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ItemsContainer t) => t.NotifyRemoveItem(default(InventoryItem)));

        private static IMultiplayerSession sessionManager;

        public static void Postfix(ItemsContainer __instance, InventoryItem item)
        {
            if (item != null && sessionManager.CurrentState.GetType() != typeof(Disconnected))
            {
                NitroxServiceLocator.LocateService<ItemContainers>().BroadcastItemRemoval(item.item, __instance.tr);
            }
        }

        public override void Patch(Harmony harmony)
        {
            sessionManager = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
