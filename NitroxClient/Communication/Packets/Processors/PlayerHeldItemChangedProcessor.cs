﻿using System;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
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

            Optional<GameObject> opItem = NitroxEntity.GetObjectFrom(packet.ItemId);
            Validate.IsPresent(opItem);

            Pickupable pickupable = opItem.Value.GetComponent<Pickupable>();
            Validate.IsTrue(pickupable);

            Validate.NotNull(pickupable.inventoryItem);

            ItemsContainer inventory = opPlayer.Value.Inventory;
            PlayerTool tool = opItem.Value.GetComponent<PlayerTool>();

            // Copied from QuickSlots
            switch (packet.Type)
            {
                case PlayerHeldItemChanged.ChangeType.DRAW_AS_TOOL:
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
                    tool.SetHandIKTargetsEnabled(true);
                    SafeAnimator.SetBool(opPlayer.Value.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", true);
                    opPlayer.Value.AnimationController["using_tool_first"] = packet.IsFirstTime == null;

                    if (opItem.Value.TryGetComponent(out FPModel fpModelDraw)) //FPModel needs to be updated
                    {
                        fpModelDraw.OnEquip(null, null);
                    }
                    break;

                case PlayerHeldItemChanged.ChangeType.HOLSTER_AS_TOOL:
                    Validate.IsTrue(tool);
                    opItem.Value.SetActive(false);
                    Utils.SetLayerRecursively(opItem.Value, defaultLayer);
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

                    if (opItem.Value.TryGetComponent(out FPModel fpModelHolster)) //FPModel needs to be updated
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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
