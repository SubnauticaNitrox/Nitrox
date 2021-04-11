using System;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerHeldItemChangedProcessor : ClientPacketProcessor<PlayerHeldItemChanged>
    {
        private readonly int defaultLayer = LayerMask.NameToLayer("Default");
        private readonly int viewModelLayer = LayerMask.NameToLayer("Viewmodel");

        private readonly FieldInfo inventoryItemFromPickupable = typeof(Pickupable).GetField("inventoryItem", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly MethodInfo setHandIK = typeof(PlayerTool).GetMethod("SetHandIKTargetsEnabled", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly PlayerManager playerManager;

        public PlayerHeldItemChangedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerHeldItemChanged packet)
        {
            Optional<RemotePlayer> opPlayer = playerManager.Find(packet.PlayerId);
            Validate.IsPresent(opPlayer);

            Optional<GameObject> opItem = NitroxEntity.GetObjectFrom(packet.ItemId);
            Validate.IsPresent(opItem);

            Pickupable pickupable = opItem.Value.GetComponent<Pickupable>();
            Validate.IsTrue(pickupable);

            InventoryItem inventoryItem = (InventoryItem)inventoryItemFromPickupable.GetValue(pickupable);
            Validate.NotNull(inventoryItem);

            ItemsContainer inventory = opPlayer.Value.Inventory;
            PlayerTool tool = opItem.Value.GetComponent<PlayerTool>();

            // Copied from QuickSlots
            switch (packet.Type)
            {
                case PlayerHeldItemChangedType.DRAW_AS_TOOL:
                    Validate.IsTrue(tool);
                    ModelPlug.PlugIntoSocket(tool, opPlayer.Value.ItemAttachPoint);
                    Utils.SetLayerRecursively(opItem.Value, viewModelLayer);
                    foreach (Animator componentsInChild in tool.GetComponentsInChildren<Animator>())
                    {
                        componentsInChild.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                    if (tool.mainCollider)
                    {
                        tool.mainCollider.enabled = false;
                    }
                    tool.GetComponent<Rigidbody>().isKinematic = true;
                    opItem.Value.SetActive(true);
                    setHandIK.Invoke(tool, new object[] { true });
                    SafeAnimator.SetBool(opPlayer.Value.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", true);

                    if (opItem.Value.TryGetComponent(out FPModel fpModelDraw)) //FPModel needs to be updated 
                    {
                        fpModelDraw.OnEquip(null, null);
                    }
                    break;

                case PlayerHeldItemChangedType.HOLSTER_AS_TOOL:
                    Validate.IsTrue(tool);
                    opItem.Value.SetActive(false);
                    Utils.SetLayerRecursively(opItem.Value, defaultLayer);
                    if (tool.mainCollider)
                    {
                        tool.mainCollider.enabled = true;
                    }
                    tool.GetComponent<Rigidbody>().isKinematic = false;
                    inventoryItem.item.Reparent(inventory.tr);
                    foreach (Animator componentsInChild in tool.GetComponentsInChildren<Animator>())
                    {
                        componentsInChild.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    }
                    SafeAnimator.SetBool(opPlayer.Value.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", false);

                    if (opItem.Value.TryGetComponent(out FPModel fpModelHolster)) //FPModel needs to be updated 
                    {
                        fpModelHolster.OnUnequip(null, null);
                    }
                    break;

                case PlayerHeldItemChangedType.DRAW_AS_ITEM:
                    inventoryItem.item.Reparent(opPlayer.Value.ItemAttachPoint);
                    inventoryItem.item.SetVisible(true);
                    Utils.SetLayerRecursively(inventoryItem.item.gameObject, viewModelLayer);
                    break;

                case PlayerHeldItemChangedType.HOLSTER_AS_ITEM:
                    inventoryItem.item.Reparent(inventory.tr);
                    inventoryItem.item.SetVisible(false);
                    Utils.SetLayerRecursively(inventoryItem.item.gameObject, defaultLayer);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
