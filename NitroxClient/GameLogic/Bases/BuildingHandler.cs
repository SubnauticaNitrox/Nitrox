using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

public partial class BuildingHandler : MonoBehaviour
{
    public static BuildingHandler Main;

    public Queue<Packet> BuildQueue;
    private bool working;

    public Dictionary<NitroxId, DateTimeOffset> BasesCooldown;

    /// <summary>
    /// When processing deconstruction-related packets, it's required to pass part of their data to the patches
    /// so that they can work accordingly (mainly to differentiate local actions from remotely issued ones).
    /// </summary>
    public TemporaryBuildData Temp;

    /// <summary>
    /// TimeSpan before which local player can build on a base that was modified by another player
    /// </summary>
    private static readonly TimeSpan MultiplayerBuildCooldown = TimeSpan.FromSeconds(2);

    public void Start()
    {
        if (Main)
        {
            Log.Error($"Another instance of {nameof(BuildingHandler)} is already running. Deleting the current one.");
            Destroy(this);
            return;
        }
        Main = this;
        BuildQueue = new();
        LatestResyncRequestTimeOffset = DateTimeOffset.UtcNow;
        BasesCooldown = new();
        Temp = new();
        Operations = new();
    }

    public void Update()
    {
        CleanCooldowns();
        if (BuildQueue.Count > 0 && !working && !Resyncing)
        {
            working = true;
            StartCoroutine(SafelyTreatNextBuildCommand());
        }
    }

    private IEnumerator SafelyTreatNextBuildCommand()
    {
        Packet packet = BuildQueue.Dequeue();
        yield return TreatBuildCommand(packet).OnYieldError(exception => Log.Error(exception, $"An error happened when processing build command {packet}"));
        working = false;
    }

    private IEnumerator TreatBuildCommand(Packet buildCommand)
    {
        switch (buildCommand)
        {
            case PlaceGhost placeGhost:
                yield return BuildGhost(placeGhost);
                break;
            case PlaceModule placeModule:
                yield return BuildModule(placeModule);
                break;
            case ModifyConstructedAmount modifyConstructedAmount:
                PeekNextModifyCommands(ref modifyConstructedAmount);
                yield return ProgressConstruction(modifyConstructedAmount);
                break;
            case UpdateBase updateBase:
                yield return UpdatePlacedBase(updateBase);
                break;
            case PlaceBase placeBase:
                yield return BuildBase(placeBase);
                break;
            case BaseDeconstructed baseDeconstructed:
                yield return DeconstructBase(baseDeconstructed);
                break;
            case PieceDeconstructed pieceDeconstructed:
                yield return DeconstructPiece(pieceDeconstructed);
                break;
            default:
                Log.Error($"Found an unhandled build command packet: {buildCommand}");
                break;
        }
    }

    /// <summary>
    /// If the next build command is also a ModifyConstructedAmount applied on the same object, we'll just skip the current one to apply the new one.
    /// </summary>
    private void PeekNextModifyCommands(ref ModifyConstructedAmount currentCommand)
    {
        while (BuildQueue.Count > 0 && BuildQueue.Peek() is ModifyConstructedAmount nextCommand && nextCommand.GhostId.Equals(currentCommand.GhostId))
        {
            BuildQueue.Dequeue();
            currentCommand = nextCommand;
        }
    }

    public IEnumerator BuildGhost(PlaceGhost placeGhost)
    {
        GhostEntity ghostEntity = placeGhost.GhostEntity;
        Transform parent = GetParentOrGlobalRoot(ghostEntity.ParentId);
        yield return GhostEntitySpawner.RestoreGhost(parent, ghostEntity);
        BasesCooldown[ghostEntity.ParentId ?? ghostEntity.Id] = DateTimeOffset.UtcNow;
    }

    public IEnumerator BuildModule(PlaceModule placeModule)
    {
        ModuleEntity moduleEntity = placeModule.ModuleEntity;
        Transform parent = GetParentOrGlobalRoot(moduleEntity.ParentId);
        TaskResult<Optional<GameObject>> result = new();
        yield return ModuleEntitySpawner.RestoreModule(parent, moduleEntity, result);
        if (result.value.HasValue)
        {
            this.Resolve<EntityMetadataManager>().ApplyMetadata(result.value.Value.gameObject, moduleEntity.Metadata);
        }
        BasesCooldown[moduleEntity.ParentId ?? moduleEntity.Id] = DateTimeOffset.UtcNow;
    }

    public IEnumerator ProgressConstruction(ModifyConstructedAmount modifyConstructedAmount)
    {
        if (NitroxEntity.TryGetComponentFrom(modifyConstructedAmount.GhostId, out Constructable constructable))
        {
            BasesCooldown[modifyConstructedAmount.GhostId] = DateTimeOffset.UtcNow;
            if (modifyConstructedAmount.ConstructedAmount == 0f)
            {
                constructable.constructedAmount = 0f;
                yield return constructable.ProgressDeconstruction();
                Destroy(constructable.gameObject);
                BasesCooldown.Remove(modifyConstructedAmount.GhostId);
                yield break;
            }
            if (modifyConstructedAmount.ConstructedAmount >= 1f)
            {
                constructable.SetState(true, true);
                yield return BuildingPostSpawner.ApplyPostSpawner(gameObject, modifyConstructedAmount.GhostId);
                yield break;
            }
            constructable.SetState(false, false);
            constructable.constructedAmount = modifyConstructedAmount.ConstructedAmount;
            yield return constructable.ProgressDeconstruction();
            constructable.UpdateMaterial();
        }
    }

    public IEnumerator BuildBase(PlaceBase placeBase)
    {
        if (!NitroxEntity.TryGetComponentFrom(placeBase.FormerGhostId, out ConstructableBase constructableBase))
        {
            FailedOperations++;
            yield break;
        }
        BaseGhost baseGhost = constructableBase.model.GetComponent<BaseGhost>();
        constructableBase.SetState(true, true);
        NitroxEntity.SetNewId(baseGhost.targetBase.gameObject, placeBase.FormerGhostId);
        BasesCooldown[placeBase.FormerGhostId] = DateTimeOffset.UtcNow;
    }

    public IEnumerator UpdatePlacedBase(UpdateBase updateBase)
    {
        if (!NitroxEntity.TryGetComponentFrom<Base>(updateBase.BaseId, out _))
        {
            Log.Error($"Couldn't find base with id: {updateBase.BaseId} when processing packet: {updateBase}");
            FailedOperations++;
            yield break;
        }

        OperationTracker tracker = EnsureTracker(updateBase.BaseId);
        tracker.RegisterOperation(updateBase.OperationId);

        if (!NitroxEntity.TryGetComponentFrom(updateBase.FormerGhostId, out ConstructableBase constructableBase))
        {
            tracker.FailedOperations++;
            Log.Error($"Couldn't find ghost with id: {updateBase.FormerGhostId} when processing packet: {updateBase}");
            yield break;
        }
        Temp.ChildrenTransfer = updateBase.ChildrenTransfer;

        BaseGhost baseGhost = constructableBase.model.GetComponent<BaseGhost>();
        constructableBase.SetState(true, true);
        BasesCooldown[updateBase.BaseId] = DateTimeOffset.UtcNow;
        // In the case the built piece was an interior piece, we'll want to transfer the id to it.
        if (BuildUtils.TryTransferIdFromGhostToModule(baseGhost, updateBase.FormerGhostId, constructableBase, out GameObject moduleObject))
        {
            yield return BuildingPostSpawner.ApplyPostSpawner(moduleObject, updateBase.FormerGhostId);
        }
    }

    public IEnumerator DeconstructBase(BaseDeconstructed baseDeconstructed)
    {
        if (!NitroxEntity.TryGetObjectFrom(baseDeconstructed.FormerBaseId, out GameObject baseObject))
        {
            FailedOperations++;
            Log.Error($"Couldn't find base with id: {baseDeconstructed.FormerBaseId} when processing packet: {baseDeconstructed}");
            yield break;
        }
        BaseDeconstructable[] deconstructableChildren = baseObject.GetComponentsInChildren<BaseDeconstructable>(true);

        if (deconstructableChildren.Length == 1 && deconstructableChildren[0])
        {
            using (PacketSuppressor<BaseDeconstructed>.Suppress())
            using (PacketSuppressor<PieceDeconstructed>.Suppress())
            {
                deconstructableChildren[0].Deconstruct();
            }
            BasesCooldown[baseDeconstructed.FormerBaseId] = DateTimeOffset.UtcNow;
            yield break;
        }
        Log.Error($"Found multiple {nameof(BaseDeconstructable)} under base {baseObject} while there should be only one");
        EnsureTracker(baseDeconstructed.FormerBaseId).FailedOperations++;
    }

    public IEnumerator DeconstructPiece(PieceDeconstructed pieceDeconstructed)
    {
        if (!NitroxEntity.TryGetComponentFrom(pieceDeconstructed.BaseId, out Base @base))
        {
            FailedOperations++;
            Log.Error($"Couldn't find base with id: {pieceDeconstructed.BaseId} when processing packet: {pieceDeconstructed}");
            yield break;
        }

        OperationTracker tracker = EnsureTracker(pieceDeconstructed.BaseId);

        BuildPieceIdentifier pieceIdentifier = pieceDeconstructed.BuildPieceIdentifier;
        Transform cellObject = @base.GetCellObject(pieceIdentifier.BaseCell.ToUnity());
        if (!cellObject)
        {
            Log.Error($"Couldn't find cell object {pieceIdentifier.BaseCell} when destructing piece {pieceDeconstructed}");
            yield break;
        }
        BaseDeconstructable[] deconstructableChildren = cellObject.GetComponentsInChildren<BaseDeconstructable>(true);
        foreach (BaseDeconstructable baseDeconstructable in deconstructableChildren)
        {
            if (!BuildUtils.TryGetIdentifier(baseDeconstructable, out BuildPieceIdentifier identifier) || !identifier.Equals(pieceIdentifier))
            {
                continue;
            }
            using (PacketSuppressor<BaseDeconstructed>.Suppress())
            using (PacketSuppressor<PieceDeconstructed>.Suppress())
            using (PacketSuppressor<WaterParkDeconstructed>.Suppress())
            using (Temp.Fill(pieceDeconstructed))
            {
                baseDeconstructable.Deconstruct();
            }
            tracker.RegisterOperation(pieceDeconstructed.OperationId);
            BasesCooldown[pieceDeconstructed.BaseId] = DateTimeOffset.UtcNow;
            yield break;
        }
        Log.Error($"Couldn't find the right BaseDeconstructable to be destructed under {pieceDeconstructed.BaseId}");
        tracker.FailedOperations++;
    }

    public static Transform GetParentOrGlobalRoot(NitroxId id)
    {
        if (id != null && NitroxEntity.TryGetObjectFrom(id, out GameObject parentObject))
        {
            return parentObject.transform;
        }
        return LargeWorldStreamer.main.globalRoot.transform;
    }

    private void CleanCooldowns()
    {
        BasesCooldown.RemoveWhere(DateTimeOffset.UtcNow, (time, curr) => (curr - time) >= MultiplayerBuildCooldown);
    }

    public class TemporaryBuildData : IDisposable
    {
        public NitroxId Id;
        public InteriorPieceEntity NewWaterPark;
        public List<NitroxId> MovedChildrenIds;
        public (NitroxId, NitroxId) ChildrenTransfer;
        public bool Transfer;

        public void Dispose()
        {
            Id = null;
            NewWaterPark = null;
            MovedChildrenIds = null;
            ChildrenTransfer = (null, null);
            Transfer = false;
        }

        public TemporaryBuildData Fill(PieceDeconstructed pieceDeconstructed)
        {
            Id = pieceDeconstructed.PieceId;
            if (pieceDeconstructed is WaterParkDeconstructed waterParkDeconstructed)
            {
                NewWaterPark = waterParkDeconstructed.NewWaterPark;
                MovedChildrenIds = waterParkDeconstructed.MovedChildrenIds;
                Transfer = waterParkDeconstructed.Transfer;
            }
            return this;
        }
    }
}

/// <summary>
/// Building resync-related part of <see cref="BuildingHandler"/>. 
/// </summary>
public partial class BuildingHandler
{
    private static readonly TimeSpan ResyncRequestCooldown = TimeSpan.FromSeconds(10);
    private DateTimeOffset LatestResyncRequestTimeOffset;

    private Dictionary<NitroxId, OperationTracker> Operations;
    // TODO: Should be used to track total fails when more stuff is built towards resyncing
    public int FailedOperations;

    public bool Resyncing;

    public OperationTracker EnsureTracker(NitroxId baseId)
    {
        if (!Operations.TryGetValue(baseId, out OperationTracker tracker))
        {
            Operations[baseId] = tracker = new();
        }
        return tracker;
    }

    public int GetCurrentOperationIdOrDefault(NitroxId baseId)
    {
        if (baseId != null && Operations.TryGetValue(baseId, out OperationTracker tracker))
        {
            return tracker.LastOperationId + tracker.LocalOperations;
        }
        return -1;
    }

    public void StartResync<T>(Dictionary<T, int> entities) where T : Entity
    {
        Resyncing = true;
        FailedOperations = 0;
        BuildQueue.Clear();
        working = true;
        InitializeOperations(entities.ToDictionary(pair => pair.Key.Id, pair => pair.Value));
    }

    public void StopResync()
    {
        working = false;
        Resyncing = false;
    }

    public void InitializeOperations(Dictionary<NitroxId, int> operations)
    {
        foreach (KeyValuePair<NitroxId, int> pair in operations)
        {
            EnsureTracker(pair.Key).ResetToId(pair.Value);
        }
    }

    public void AskForResync()
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }
        TimeSpan deltaTime = DateTimeOffset.UtcNow - LatestResyncRequestTimeOffset;
        if (deltaTime < ResyncRequestCooldown)
        {
            double timeLeft = ResyncRequestCooldown.TotalSeconds - deltaTime.TotalSeconds;
            Log.InGame(Language.main.Get("Nitrox_ResyncOnCooldown").Replace("{TIME_LEFT}", string.Format("{0:N2}", timeLeft)));
            return;
        }
        LatestResyncRequestTimeOffset = DateTimeOffset.UtcNow;

        this.Resolve<IPacketSender>().Send(new BuildingResyncRequest());
        Log.InGame(Language.main.Get("Nitrox_ResyncRequested"));
    }
}
