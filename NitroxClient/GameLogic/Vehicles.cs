using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class Vehicles
{
    private readonly IPacketSender packetSender;
    private readonly IMultiplayerSession multiplayerSession;
    private readonly PlayerManager playerManager;
    private readonly EntityMetadataManager entityMetadataManager;
    private readonly Entities entities;

    private readonly Dictionary<TechType, string> pilotingChairByTechType = [];

    public Vehicles(IPacketSender packetSender, IMultiplayerSession multiplayerSession, PlayerManager playerManager, EntityMetadataManager entityMetadataManager, Entities entities)
    {
        this.packetSender = packetSender;
        this.multiplayerSession = multiplayerSession;
        this.playerManager = playerManager;
        this.entityMetadataManager = entityMetadataManager;
        this.entities = entities;
    }

    private PilotingChair FindPilotingChairWithCache(GameObject parent, TechType techType)
    {
        if (!parent)
        {
            return null;
        }
        if (pilotingChairByTechType.TryGetValue(techType, out string path))
        {
            if (path == string.Empty)
            {
                return null;
            }
            return parent.transform.Find(path).GetComponent<PilotingChair>();
        }
        else
        {
            PilotingChair chair = parent.GetComponentInChildren<PilotingChair>(true);
            pilotingChairByTechType.Add(techType, chair ? chair.gameObject.GetHierarchyPath(parent) : string.Empty);
            return chair;
        }
    }

    public void BroadcastDestroyedVehicle(NitroxId id)
    {
        using (PacketSuppressor<VehicleOnPilotModeChanged>.Suppress())
        {
            EntityDestroyed entityDestroyed = new(id);
            packetSender.Send(entityDestroyed);
        }
    }

    public void BroadcastDestroyedCyclops(GameObject cyclops, NitroxId id)
    {
        CyclopsMetadataExtractor.CyclopsGameObject cyclopsGameObject = new() { GameObject = cyclops };
        Optional<EntityMetadata> metadata = entityMetadataManager.Extract(cyclopsGameObject);

        if (metadata.HasValue && metadata.Value is CyclopsMetadata cyclopsMetadata)
        {
            cyclopsMetadata.IsDestroyed = true;
            entities.BroadcastMetadataUpdate(id, cyclopsMetadata);
        }
    }
#if SUBNAUTICA
    public static void EngagePlayerMovementSuppressor(Vehicle vehicle)
#elif BELOWZERO
    public static void EngagePlayerMovementSuppressor(Dockable vehicle)
#endif
    {
        // TODO: Properly prevent the vehicle from sending position update as long as it's not free from the animation
        PacketSuppressor<PlayerMovement> playerMovementSuppressor = PacketSuppressor<PlayerMovement>.Suppress();
        vehicle.StartCoroutine(AllowMovementPacketsAfterDockingAnimation());
        return;

        /*
         A poorly timed movement packet will cause major problems when docking because the remote
         player will think that the player is no longer in a vehicle.  Unfortunately, the game calls
         the vehicle exit code before the animation completes so we need to suppress any side effects.
         Two thing we want to protect against:

             1) If a movement packet is received when docking, the player might exit the vehicle early,
                and it will show them sitting outside the vehicle during the docking animation.

             2) If a movement packet is received when undocking, the player game object will be stuck in
                place until after the player exits the vehicle.  This causes the player body to stretch to
                the current cyclops position.
        */
        IEnumerator AllowMovementPacketsAfterDockingAnimation()
        {
            yield return Yielders.WaitFor3Seconds;
            playerMovementSuppressor.Dispose();
        }
    }

    public void BroadcastOnPilotModeChanged(GameObject gameObject, bool isPiloting)
    {
        if (gameObject.TryGetIdOrWarn(out NitroxId vehicleId))
        {
            VehicleOnPilotModeChanged packet = new(vehicleId, multiplayerSession.Reservation.PlayerId, isPiloting);
            packetSender.Send(packet);
        }
    }

    public void SetOnPilotMode(GameObject gameObject, ushort playerId, bool isPiloting)
    {
        if (playerManager.TryFind(playerId, out RemotePlayer remotePlayer))
        {
            if (gameObject.TryGetComponent(out Vehicle vehicle))
            {
                remotePlayer.SetVehicle(isPiloting ? vehicle : null);
            }
            else if (gameObject.GetComponent<SubRoot>())
            {
                if (!isPiloting)
                {
                    remotePlayer.SetPilotingChair(null);
                    return;
                }
                PilotingChair pilotingChair = FindPilotingChairWithCache(gameObject, TechType.Cyclops);
                remotePlayer.SetPilotingChair(pilotingChair);
            }
            // TODO: [FUTURE] For any mods adding new vehicle with a piloting chair, there should be something done right here
        }
    }

    public void SetOnPilotMode(NitroxId vehicleId, ushort playerId, bool isPiloting)
    {
        if (NitroxEntity.TryGetObjectFrom(vehicleId, out GameObject vehicleObject))
        {
            SetOnPilotMode(vehicleObject, playerId, isPiloting);
        }
    }

    /// <summary>
    /// Removes ALL <see cref="NitroxEntity"/> on the <see cref="GameObject"/> and its children.
    /// </summary>
    /// <remarks>
    /// Subnautica pre-emptively loads a prefab of each vehicle (such as a cyclops) during the initial game load.  This allows the game to instantaniously
    /// use this prefab for the first constructor event.  Subsequent constructor events will use this prefab as a template.  However, this is problematic
    /// because the template + children are now tagged with NitroxEntity because players are interacting with it. We need to remove any NitroxEntity from
    /// the new gameObject that used the template.
    /// </remarks>
    public static void RemoveNitroxEntitiesTagging(GameObject constructedObject)
    {
        NitroxEntity[] nitroxEntities = constructedObject.GetComponentsInChildren<NitroxEntity>(true);

        foreach (NitroxEntity nitroxEntity in nitroxEntities)
        {
            nitroxEntity.Remove();
            Object.DestroyImmediate(nitroxEntity);
        }
    }

    public static VehicleWorldEntity BuildVehicleWorldEntity(GameObject constructedObject, NitroxId constructedObjectId, TechType techType, NitroxId constructorId = null)
    {
        VehicleWorldEntity vehicleEntity = new(constructorId, DayNightCycle.main.timePassedAsFloat, constructedObject.transform.ToLocalDto(), string.Empty, false, constructedObjectId, techType.ToDto(), null);
        VehicleChildEntityHelper.PopulateChildren(constructedObjectId, constructedObject.GetFullHierarchyPath(), vehicleEntity.ChildEntities, constructedObject);
        return vehicleEntity;
    }
}
