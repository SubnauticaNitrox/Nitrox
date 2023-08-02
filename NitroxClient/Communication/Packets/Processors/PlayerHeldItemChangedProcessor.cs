using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerHeldItemChangedProcessor : ClientPacketProcessor<PlayerHeldItemChanged>
{
    private int defaultLayer;
    private int viewModelLayer;
    private readonly PlayerManager playerManager;

    public PlayerHeldItemChangedProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;

        if (NitroxEnvironment.IsNormal)
        {
            SetupLayers();
        }
    }

    private void SetupLayers()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        viewModelLayer = LayerMask.NameToLayer("Viewmodel");
    }

    public override void Process(PlayerHeldItemChanged packet)
    {
        Optional<RemotePlayer> opPlayer = playerManager.Find(packet.PlayerId);
        Validate.IsPresent(opPlayer);

        if (!NitroxEntity.TryGetObjectFrom(packet.ItemId, out GameObject item))
        {
            return; // Item can be not spawned yet bc async.
        }

        Pickupable pickupable = item.GetComponent<Pickupable>();
        Validate.IsTrue(pickupable);

        Validate.NotNull(pickupable.inventoryItem);

        ItemsContainer inventory = opPlayer.Value.Inventory;
        PlayerTool tool = item.GetComponent<PlayerTool>();

        // Copied from QuickSlots
        switch (packet.Type)
        {
            case PlayerHeldItemChanged.ChangeType.DRAW_AS_TOOL:
                Validate.IsTrue(tool);
                ModelPlug.PlugIntoSocket(tool, opPlayer.Value.ItemAttachPoint);
                Utils.SetLayerRecursively(item, viewModelLayer);
                foreach (Animator componentsInChild in tool.GetComponentsInChildren<Animator>())
                {
                    componentsInChild.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
                if (tool.mainCollider)
                {
                    tool.mainCollider.enabled = false;
                }
                tool.GetComponent<Rigidbody>().isKinematic = true;
                item.SetActive(true);
                tool.SetHandIKTargetsEnabled(true);
                SafeAnimator.SetBool(opPlayer.Value.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", true);
                opPlayer.Value.AnimationController["using_tool_first"] = packet.IsFirstTime == null;

                if (item.TryGetComponent(out FPModel fpModelDraw)) //FPModel needs to be updated
                {
                    fpModelDraw.OnEquip(null, null);
                }
                break;

            case PlayerHeldItemChanged.ChangeType.HOLSTER_AS_TOOL:
                Validate.IsTrue(tool);
                item.SetActive(false);
                Utils.SetLayerRecursively(item, defaultLayer);
                if (tool.mainCollider)
                {
                    tool.mainCollider.enabled = true;
                }
                tool.GetComponent<Rigidbody>().isKinematic = false;
                pickupable.inventoryItem.item.Reparent(inventory.tr);
                foreach (Animator componentsInChild in tool.GetComponentsInChildren<Animator>())
                {
                    componentsInChild.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                }
                SafeAnimator.SetBool(opPlayer.Value.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", false);
                opPlayer.Value.AnimationController["using_tool_first"] = false;

                if (item.TryGetComponent(out FPModel fpModelHolster)) //FPModel needs to be updated
                {
                    fpModelHolster.OnUnequip(null, null);
                }

                break;

            case PlayerHeldItemChanged.ChangeType.DRAW_AS_ITEM:
                pickupable.inventoryItem.item.Reparent(opPlayer.Value.ItemAttachPoint);
                pickupable.inventoryItem.item.SetVisible(true);
                Utils.SetLayerRecursively(pickupable.inventoryItem.item.gameObject, viewModelLayer);
                break;

            case PlayerHeldItemChanged.ChangeType.HOLSTER_AS_ITEM:
                pickupable.inventoryItem.item.Reparent(inventory.tr);
                pickupable.inventoryItem.item.SetVisible(false);
                Utils.SetLayerRecursively(pickupable.inventoryItem.item.gameObject, defaultLayer);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(packet.Type));
        }
    }
}
