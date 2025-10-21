using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;
using Nitrox.Model.Subnautica.Extensions;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityDestroyedProcessor : ClientPacketProcessor<EntityDestroyed>
{
    public const DamageType DAMAGE_TYPE_RUN_ORIGINAL = (DamageType)100;

    private readonly Entities entities;

    public EntityDestroyedProcessor(Entities entities)
    {
        this.entities = entities;
    }

    public override void Process(EntityDestroyed packet)
    {
        bool wasKnownEntity = entities.IsKnownEntity(packet.Id);
        entities.RemoveEntity(packet.Id);
        
        if (entities.SpawningEntities)
        {
            entities.MarkForDeletion(packet.Id);
        }

        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject gameObject))
        {
            if (wasKnownEntity)
            {
                Log.Warn($"[{nameof(EntityDestroyedProcessor)}] Could not find entity with id: {packet.Id} to destroy.");
            }

            RemoveScannerPingsNear(packet);
            RemoveMapRoomNodesNear(packet);
            return;
        }

        using (PacketSuppressor<EntityDestroyed>.Suppress())
        {
            // This type of check could get out of control if there are many types with custom destroy logic.  If we get a few more, move to separate processors.
            if (gameObject.TryGetComponent(out Vehicle vehicle))
            {
                DestroyVehicle(vehicle);
            }
            else if (gameObject.TryGetComponent(out SubRoot subRoot))
            {
                DestroySubroot(subRoot);
            }
            else
            {
                Entities.DestroyObject(gameObject);
            }
        }

        RemoveScannerPingsNear(packet);
        RemoveMapRoomNodesNear(packet);
    }

    private static void RemoveScannerPingsNear(EntityDestroyed packet)
    {
        if (!packet.LastKnownPosition.HasValue)
        {
            return;
        }

        Vector3 targetPosition = packet.LastKnownPosition.Value.ToUnity();
        const float removalRadius = 2f;

        PingInstance[] pingInstances = UnityEngine.Object.FindObjectsOfType<PingInstance>();

        foreach (PingInstance ping in pingInstances)
        {
            string pingTypeName = ping.pingType.ToString();
            if (pingTypeName.IndexOf("MapRoom", StringComparison.OrdinalIgnoreCase) < 0 &&
                pingTypeName.IndexOf("Scanner", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            if (Vector3.Distance(ping.transform.position, targetPosition) > removalRadius)
            {
                continue;
            }

            UnityEngine.Object.Destroy(ping.gameObject);
        }
    }

    private static void RemoveMapRoomNodesNear(EntityDestroyed packet)
    {
        if (!packet.LastKnownPosition.HasValue)
        {
            return;
        }

        Vector3 targetPosition = packet.LastKnownPosition.Value.ToUnity();
        TechType? destroyedTechType = packet.TechType != null ? packet.TechType.ToUnity() : null;
        const float removalRadius = 4f;

        MapRoomFunctionality[] mapRooms = UnityEngine.Object.FindObjectsOfType<MapRoomFunctionality>();
        foreach (MapRoomFunctionality mapRoom in mapRooms)
        {
            if (!mapRoom)
            {
                continue;
            }

            IList<ResourceTrackerDatabase.ResourceInfo> nodes = mapRoom.GetNodes();
            if (nodes == null || nodes.Count == 0)
            {
                continue;
            }

            ResourceTrackerDatabase.ResourceInfo[] snapshot = nodes.ToArray();
            foreach (ResourceTrackerDatabase.ResourceInfo info in snapshot)
            {
                if (destroyedTechType.HasValue && info.techType != destroyedTechType.Value)
                {
                    continue;
                }

                if (Vector3.Distance(info.position, targetPosition) > removalRadius)
                {
                    continue;
                }

                mapRoom.OnResourceRemoved(info);
            }
        }
    }

    private void DestroySubroot(SubRoot subRoot)
    {
        DamageInfo damageInfo = new() { type = DAMAGE_TYPE_RUN_ORIGINAL };
        if (subRoot.live.health > 0f)
        {
            // oldHPPercent must be in the interval [0; 0.25[ because else, SubRoot.OnTakeDamage will end up in the wrong else condition
            subRoot.oldHPPercent = 0f;
            subRoot.live.health = 0f;
            subRoot.live.NotifyAllAttachedDamageReceivers(damageInfo);
            subRoot.live.Kill();
        }

        // We use a specific DamageType so that the Prefix on this method will accept this call
        subRoot.OnTakeDamage(damageInfo);
    }

    private void DestroyVehicle(Vehicle vehicle)
    {
        if (vehicle.GetPilotingMode()) //Check Local Object Have Player inside
        {
            vehicle.OnPilotModeEnd();

            if (!Player.main.ToNormalMode(true))
            {
                Player.main.ToNormalMode(false);
                Player.main.transform.parent = null;
            }
        }

        foreach (RemotePlayerIdentifier identifier in vehicle.GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            identifier.RemotePlayer.ResetStates();
        }

        if (vehicle.gameObject)
        {
            if (vehicle.destructionEffect)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(vehicle.destructionEffect);
                gameObject.transform.position = vehicle.transform.position;
                gameObject.transform.rotation = vehicle.transform.rotation;
            }

            UnityEngine.Object.Destroy(vehicle.gameObject);
        }
    }
}
