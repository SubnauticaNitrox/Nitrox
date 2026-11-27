using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerHeldItemChangedProcessor : IClientPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerManager playerManager;
    private readonly Dictionary<SessionId, long> latestPacketSequenceBySession = [];
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
        long sequence = latestPacketSequenceBySession.GetValueOrDefault(packet.SessionId) + 1;
        latestPacketSequenceBySession[packet.SessionId] = sequence;

        if (!playerManager.TryFind(packet.SessionId, out RemotePlayer player))
        {
            Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Could not find player with session id: {packet.SessionId}.");
            return Task.CompletedTask;
        }
        if (!NitroxEntity.TryGetObjectFrom(packet.ItemId, out GameObject item))
        {
            // Auto-equipped items can be announced before their entity is spawned on this client.
            // Wait for that entity, but discard this update if a newer held-item state arrives first.
            CoroutineHost.StartCoroutine(ApplyWhenItemIsReady(packet, sequence));
            return Task.CompletedTask;
        }

        Apply(player, packet, item);
        return Task.CompletedTask;
    }

    private IEnumerator ApplyWhenItemIsReady(PlayerHeldItemChanged packet, long sequence)
    {
        while (playerManager.TryFind(packet.SessionId, out RemotePlayer player) &&
               latestPacketSequenceBySession.GetValueOrDefault(packet.SessionId) == sequence &&
               !NitroxEntity.TryGetObjectFrom(packet.ItemId, out _))
        {
            yield return null;
        }

        if (playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer) &&
            latestPacketSequenceBySession.GetValueOrDefault(packet.SessionId) == sequence &&
            NitroxEntity.TryGetObjectFrom(packet.ItemId, out GameObject item))
        {
            Apply(remotePlayer, packet, item);
        }
    }

    private void Apply(RemotePlayer player, PlayerHeldItemChanged packet, GameObject item)
    {
        Pickupable pickupable = item.GetComponent<Pickupable>();
        Validate.IsTrue(pickupable);

        Validate.NotNull(pickupable.inventoryItem);

        ItemsContainer inventory = player.Inventory;
        PlayerTool tool = item.GetComponent<PlayerTool>();

        // Copied from QuickSlots
        switch (packet.Type)
        {
            case PlayerHeldItemChanged.ChangeType.DRAW_AS_TOOL:
                Validate.IsTrue(tool);
                ModelPlug.PlugIntoSocket(tool, player.ItemAttachPoint);
                Utils.SetLayerRecursively(item, viewModelLayer);
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
                SafeAnimator.SetBool(player.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", true);
                player.AnimationController["using_tool_first"] = packet.IsFirstTime != null;

                if (item.TryGetComponent(out FPModel fpModelDraw)) //FPModel needs to be updated
                {
                    fpModelDraw.OnEquip(null, null);
                }
                SetToolAnimatorCullingMode(tool, AnimatorCullingMode.AlwaysAnimate);
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
                SafeAnimator.SetBool(player.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", false);
                player.AnimationController["using_tool_first"] = false;

                if (item.TryGetComponent(out FPModel fpModelHolster)) //FPModel needs to be updated
                {
                    fpModelHolster.OnUnequip(null, null);
                }
                SetToolAnimatorCullingMode(tool, AnimatorCullingMode.CullUpdateTransforms);

                break;

            case PlayerHeldItemChanged.ChangeType.DRAW_AS_ITEM:
                pickupable.inventoryItem.item.Reparent(player.ItemAttachPoint);
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

    private static void SetToolAnimatorCullingMode(PlayerTool tool, AnimatorCullingMode cullingMode)
    {
        foreach (Animator animator in tool.GetComponentsInChildren<Animator>(true))
        {
            animator.cullingMode = cullingMode;
        }

        if (tool.TryGetComponent(out FPModel fpModel) && fpModel.viewModel)
        {
            foreach (Animator animator in fpModel.viewModel.GetComponentsInChildren<Animator>(true))
            {
                animator.cullingMode = cullingMode;
            }
        }
    }

    private void SetupLayers()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        viewModelLayer = LayerMask.NameToLayer("Viewmodel");
    }
}
