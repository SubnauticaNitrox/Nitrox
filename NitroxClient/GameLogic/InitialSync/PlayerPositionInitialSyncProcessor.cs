using System.Collections;
using NitroxClient.Communication;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Util;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using UWE;
using Math = System.Math;

namespace NitroxClient.GameLogic.InitialSync;

public sealed class PlayerPositionInitialSyncProcessor : InitialSyncProcessor
{
    private static readonly Vector3 spawnRelativeToEscapePod = new(0.9f, 2.1f, 0);
    private readonly Entities entities;

    public PlayerPositionInitialSyncProcessor(Entities entities)
    {
        this.entities = entities;

        AddDependency<PlayerInitialSyncProcessor>();
        AddDependency<GlobalRootInitialSyncProcessor>();
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        // We freeze the player so that he doesn't fall before the cells around him have loaded
        // Is disabled manually or in Terrain.WaitForWorldLoad()
        Player.main.cinematicModeActive = true;

        AttachPlayerToEscapePod(packet.AssignedEscapePodId);

        Vector3 position = packet.PlayerSpawnData.ToUnity();
        Quaternion rotation = packet.PlayerSpawnRotation.ToUnity();
        if (Math.Abs(position.x) < 0.0002 && Math.Abs(position.y) < 0.0002 && Math.Abs(position.z) < 0.0002)
        {
            position = Player.mainObject.transform.position;
        }
        Player.main.SetPosition(position, rotation);

        // Player.ValidateEscapePod is setting currentEscapePod to null if player is not inside EscapePod
        using (PacketSuppressor<EscapePodChanged>.Suppress())
        {
            Player.main.ValidateEscapePod();
        }

        Player.main.precursorOutOfWater = packet.InPrecursor;
        Player.main.SetDisplaySurfaceWater(packet.DisplaySurfaceWater);

        Optional<NitroxId> subRootId = packet.PlayerSubRootId;
        if (!subRootId.HasValue)
        {
            yield return Terrain.SafeWaitForWorldLoad();
            Player.main.UpdateIsUnderwater();

            // Check if Player might fall through the map
            if (packet.InPrecursor || !Player.main.IsUnderwaterForSwimming())
            {
                // This coroutine must be started fresh to release the current one
                CoroutineHost.StartCoroutine(FreezePlayerWhileEntitiesSpawn());
            }
            
            yield break;
        }

        Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subRootId.Value);
        if (!sub.HasValue)
        {
            Log.Error($"Could not spawn player into subroot with id: {subRootId.Value}");
            yield return Terrain.SafeWaitForWorldLoad();
            yield break;
        }

        if (sub.Value.TryGetComponent(out SubRoot subRoot))
        {
            Player.main.SetCurrentSub(subRoot, true);
            if (subRoot.TryGetComponent(out Base @base))
            {
                SetupPlayerIfInWaterPark(@base);
            }
        }
        else if (sub.Value.GetComponent<EscapePod>())
        {
            Player.main.escapePod.Update(true);
            Player.main.ApplyEscapePodSkyIfNeeded();
        }
        else
        {
            Log.Error("SubRootId-GameObject has no SubRoot or EscapePod component");
        }

        // If the player's in a base/cyclops we don't need to wait for the world to load
        Player.main.UpdateIsUnderwater();
        Player.main.cinematicModeActive = false;
    }

    private static void AttachPlayerToEscapePod(NitroxId escapePodId)
    {
        GameObject escapePod = NitroxEntity.RequireObjectFrom(escapePodId);

        EscapePod.main.transform.position = escapePod.transform.position;
        EscapePod.main.playerSpawn.position = escapePod.transform.position + spawnRelativeToEscapePod;

        Player.main.transform.position = EscapePod.main.playerSpawn.position;
        Player.main.transform.rotation = EscapePod.main.playerSpawn.rotation;

        Player.main.currentEscapePod = escapePod.GetComponent<EscapePod>();
    }

    private static void SetupPlayerIfInWaterPark(Base @base)
    {
        foreach (Transform baseChild in @base.transform)
        {
            if (baseChild.TryGetComponent(out WaterPark waterPark))
            {
                if (waterPark is LargeRoomWaterPark)
                {
                    // LargeRoomWaterPark.VerifyPlayerWaterPark sets Player.main.currentWaterPark to the right value
                    waterPark.VerifyPlayerWaterPark(Player.main);
                }
                else if (waterPark.IsPointInside(Player.main.transform.position))
                {
                    Player.main.currentWaterPark = waterPark;
                }
            }

            if (Player.main.currentWaterPark)
            {
                return;
            }
        }
    }

    private IEnumerator FreezePlayerWhileEntitiesSpawn()
    {
        Player.main.cinematicModeActive = true;

        float timeStartWait = DayNightCycle.main.timePassedAsFloat;

        // Either wait for SpawnEntities packet to arrive or time out this freeze because we don't expect entities to arrive after that
        yield return new WaitUntil(() => entities.SpawningEntities || DayNightCycle.main.timePassedAsFloat - timeStartWait > 5);

        yield return new WaitUntil(() => !entities.SpawningEntities);

        Player.main.cinematicModeActive = false;
    }
}
