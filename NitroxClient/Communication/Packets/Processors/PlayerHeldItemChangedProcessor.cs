using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerHeldItemChangedProcessor : IClientPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerManager playerManager;
    private int defaultLayer;
    private int viewModelLayer;

    public PlayerHeldItemChangedProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;

        if (NitroxEnvironment.IsNormal)
        {
            SetupLayers();
        }
    }

    public Task Process(ClientProcessorContext context, PlayerHeldItemChanged packet)
    {
        Optional<RemotePlayer> opPlayer = playerManager.Find(packet.SessionId);
        Validate.IsPresent(opPlayer);

        if (!NitroxEntity.TryGetObjectFrom(packet.ItemId, out GameObject item))
        {
            return Task.CompletedTask;
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
                if (tool.TryGetComponent(out Floater floater))
                {
                    floater.collider.enabled = false;
                }
                item.SetActive(true);
                tool.SetHandIKTargetsEnabled(true);
                SafeAnimator.SetBool(opPlayer.Value.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", true);
                opPlayer.Value.AnimationController["using_tool_first"] = packet.IsFirstTime != null;

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
                if (tool.TryGetComponent(out floater))
                {
                    floater.collider.enabled = true;
                }
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
        return Task.CompletedTask;
    }

    private void SetupLayers()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        viewModelLayer = LayerMask.NameToLayer("Viewmodel");
    }
}
