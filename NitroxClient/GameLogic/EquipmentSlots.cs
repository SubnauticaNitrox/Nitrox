using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class EquipmentSlots
{
    private readonly IPacketSender packetSender;
    private readonly Entities entities;
    private readonly EntityMetadataManager entityMetadataManager;

    public EquipmentSlots(IPacketSender packetSender, Entities entities, EntityMetadataManager entityMetadataManager)
    {
        this.packetSender = packetSender;
        this.entities = entities;
        this.entityMetadataManager = entityMetadataManager;
    }

    public void BroadcastEquip(Pickupable pickupable, GameObject owner, string slot)
    {
        if (!owner.TryGetIdOrWarn(out NitroxId ownerId))
        {
            return;
        }
        if (!pickupable.TryGetIdOrWarn(out NitroxId itemId))
        {
            return;
        }

        if (owner.TryGetComponent(out Player player))
        {
            entities.EntityMetadataChanged(player, ownerId);
        }
        else
        {
            // UWE also sends module events here as they are technically equipment of the vehicles.
            string classId = pickupable.RequireComponent<PrefabIdentifier>().ClassId;
            NitroxTechType techType = pickupable.GetTechType().ToDto();
            EntityMetadata metadata = entityMetadataManager.Extract(pickupable.gameObject).OrNull();

            InstalledModuleEntity moduleEntity = new(slot, classId, itemId, techType, metadata, ownerId, []);

            if (packetSender.Send(new EntitySpawnedByClient(moduleEntity, true)))
            {
                Log.Debug($"Sent: Added module {pickupable.GetTechType()} ({itemId}) to equipment {owner.GetFullHierarchyPath()} in slot {slot}");
            }
        }
    }

    public void BroadcastUnequip(Pickupable pickupable, GameObject owner)
    {
        if (!pickupable.TryGetIdOrWarn(out NitroxId itemId))
        {
            return;
        }
        if (!owner.TryGetIdOrWarn(out NitroxId ownerId))
        {
            return;
        }

        if (owner.TryGetComponent(out Player player))
        {
            entities.EntityMetadataChanged(player, ownerId);
        }
        else
        {
            // UWE also sends module events here as they are technically equipment of the vehicles.
            packetSender.Send(new EntityDestroyed(itemId));
        }
    }
}
