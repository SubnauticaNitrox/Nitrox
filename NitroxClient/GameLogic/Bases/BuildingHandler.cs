using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

public class BuildingHandler : MonoBehaviour
{
    public static BuildingHandler Main;

    public Queue<Packet> BuildQueue;
    private bool working;

    public Dictionary<NitroxId, DateTimeOffset> BasesCooldown;
    public DateTimeOffset LatestResyncRequestTimeOffset;

    public TemporaryBuildData Temp;
    public Dictionary<NitroxId, OperationTracker> Operations;
    public int FailedOperations;

    public bool Resyncing;

    /// <summary>
    /// Time in milliseconds before local player can build on a base that was modified by another player
    /// </summary>
    private const int MULTIPLAYER_BUILD_COOLDOWN = 2000;
    private const int RESYNC_REQUEST_COOLDOWN = 10000;

    public void Start()
    {
        Main = this;
        BuildQueue = new();
        LatestResyncRequestTimeOffset = DateTimeOffset.Now;
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
        yield return TreatBuildCommand(packet).OnYieldError(exception => Log.Error($"An error happened when treating build command {packet}:\n{exception}"));
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
                PeekNextCommand(ref modifyConstructedAmount);
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
        }
    }

    /// <summary>
    /// If the next build command is also a ModifyConstructedAmount applied on the same object, we'll just skip the current one to apply the new one.
    /// </summary>
    private void PeekNextCommand(ref ModifyConstructedAmount currentCommand)
    {
        if (BuildQueue.Count == 0 || BuildQueue.Peek() is not ModifyConstructedAmount nextCommand || !nextCommand.GhostId.Equals(currentCommand.GhostId))
        {
            return;
        }
        BuildQueue.Dequeue();
        currentCommand = nextCommand;
        PeekNextCommand(ref currentCommand);
    }

    public IEnumerator BuildGhost(PlaceGhost placeGhost)
    {
        GhostEntity ghostEntity = placeGhost.GhostEntity;

        Transform parent = GetParentOrGlobalRoot(ghostEntity.ParentId);
        yield return GhostEntitySpawner.RestoreGhost(parent, ghostEntity);
        BasesCooldown[ghostEntity.ParentId ?? ghostEntity.Id] = DateTimeOffset.Now;
    }

    public IEnumerator BuildModule(PlaceModule placeModule)
    {
        ModuleEntity moduleEntity = placeModule.ModuleEntity;
        Transform parent = GetParentOrGlobalRoot(moduleEntity.ParentId);
        yield return ModuleEntitySpawner.RestoreModule(parent, moduleEntity);
        BasesCooldown[moduleEntity.ParentId ?? moduleEntity.Id] = DateTimeOffset.Now;
    }

    public IEnumerator ProgressConstruction(ModifyConstructedAmount modifyConstructedAmount)
    {
        if (NitroxEntity.TryGetComponentFrom(modifyConstructedAmount.GhostId, out Constructable constructable))
        {
            BasesCooldown[modifyConstructedAmount.GhostId] = DateTimeOffset.Now;
            if (modifyConstructedAmount.ConstructedAmount == 0f)
            {
                constructable.constructedAmount = 0f;
                yield return constructable.ProgressDeconstruction();
                Destroy(constructable.gameObject);
                BasesCooldown.Remove(modifyConstructedAmount.GhostId);
                yield break;
            }
            else if (modifyConstructedAmount.ConstructedAmount == 1f)
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
        if (NitroxEntity.TryGetComponentFrom(placeBase.FormerGhostId, out ConstructableBase constructableBase))
        {
            BaseGhost baseGhost = constructableBase.model.GetComponent<BaseGhost>();
            constructableBase.SetState(true, true);
            NitroxEntity.SetNewId(baseGhost.targetBase.gameObject, placeBase.FormerGhostId);
            BasesCooldown[placeBase.FormerGhostId] = DateTimeOffset.Now;
            yield break;
        }
        FailedOperations++;
    }

    public IEnumerator UpdatePlacedBase(UpdateBase updateBase)
    {
        // Is probably useless check but can probably be useful for desync detection
        if (!NitroxEntity.TryGetComponentFrom<Base>(updateBase.BaseId, out _))
        {
            Log.Debug("Couldn't find the parentBase");
            FailedOperations++;
            yield break;
        }

        OperationTracker tracker = EnsureTracker(updateBase.BaseId);
        tracker.RegisterOperation(updateBase.OperationId);

        if (NitroxEntity.TryGetComponentFrom(updateBase.FormerGhostId, out ConstructableBase constructableBase))
        {
            Temp.ChildrenTransfer = updateBase.ChildrenTransfer;

            BaseGhost baseGhost = constructableBase.model.GetComponent<BaseGhost>();
            constructableBase.SetState(true, true);
            BasesCooldown[updateBase.BaseId] = DateTimeOffset.Now;
            // In the case the built piece was an interior piece, we'll want to transfer the id to it.
            if (BuildUtils.TryTransferIdFromGhostToModule(baseGhost, updateBase.FormerGhostId, constructableBase, out GameObject moduleObject))
            {
                yield return BuildingPostSpawner.ApplyPostSpawner(moduleObject, updateBase.FormerGhostId);
            }
            yield break;
        }
        tracker.FailedOperations++;
    }

    public IEnumerator DeconstructBase(BaseDeconstructed baseDeconstructed)
    {
        if (!NitroxEntity.TryGetObjectFrom(baseDeconstructed.FormerBaseId, out GameObject baseObject))
        {
            FailedOperations++;
            Log.Debug("Couldn't find the parentBase");
            yield break;
        }
        BaseDeconstructable[] deconstructableChildren = baseObject.GetComponentsInChildren<BaseDeconstructable>(true);
        Log.Debug($"Found {deconstructableChildren.Length} BaseDeconstructable in the base");

        if (deconstructableChildren.Length == 1 && deconstructableChildren[0])
        {
            Log.Debug("Will only deconstruct the base manually");
            using (new PacketSuppressor<BaseDeconstructed>())
            using (new PacketSuppressor<PieceDeconstructed>())
            {
                deconstructableChildren[0].Deconstruct();
            }
            BasesCooldown[baseDeconstructed.FormerBaseId] = DateTimeOffset.Now;
            yield break;
        }
        EnsureTracker(baseDeconstructed.FormerBaseId).FailedOperations++;
    }

    public IEnumerator DeconstructPiece(PieceDeconstructed pieceDeconstructed)
    {
        if (!NitroxEntity.TryGetComponentFrom(pieceDeconstructed.BaseId, out Base @base))
        {
            FailedOperations++;
            Log.Debug("Couldn't find the parentBase");
            yield break;
        }

        OperationTracker tracker = EnsureTracker(pieceDeconstructed.BaseId);

        BuildPieceIdentifier pieceIdentifier = pieceDeconstructed.BuildPieceIdentifier;
        Transform cellObject = @base.GetCellObject(pieceIdentifier.BaseCell.ToUnity());
        if (!cellObject)
        {
            Log.Debug($"Couldn't find cell object {pieceIdentifier.BaseCell} when destructing piece {pieceDeconstructed}");
            yield break;
        }
        BaseDeconstructable[] deconstructableChildren = cellObject.GetComponentsInChildren<BaseDeconstructable>(true);
        foreach (BaseDeconstructable baseDeconstructable in deconstructableChildren)
        {
            if (!BuildUtils.TryGetIdentifier(baseDeconstructable, out BuildPieceIdentifier identifier) || !identifier.Equals(pieceIdentifier))
            {
                continue;
            }
            Log.Debug($"[{pieceDeconstructed.OperationId}] Found a BaseDeconstructable {baseDeconstructable.name}, will now deconstruct it manually");
            using (PacketSuppressor<BaseDeconstructed>.Suppress())
            using (PacketSuppressor<PieceDeconstructed>.Suppress())
            using (PacketSuppressor<WaterParkDeconstructed>.Suppress())
            {
                Temp.Fill(pieceDeconstructed);
                baseDeconstructable.Deconstruct();
                Temp.Reset();
            }
            tracker.RegisterOperation(pieceDeconstructed.OperationId);
            BasesCooldown[pieceDeconstructed.BaseId] = DateTimeOffset.Now;
            yield break;
        }
        Log.Error("Couldn't find BaseDeconstructable to be destructed");
        tracker.FailedOperations++;
    }

    public void AskForResync()
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }
        TimeSpan deltaTime = DateTimeOffset.Now - LatestResyncRequestTimeOffset;
        if (deltaTime.TotalMilliseconds < RESYNC_REQUEST_COOLDOWN)
        {
            double timeLeft = RESYNC_REQUEST_COOLDOWN * 0.001 - deltaTime.TotalSeconds;
            ErrorMessage.AddMessage($"On cooldown for resync request for {timeLeft} more seconds");
            return;
        }
        LatestResyncRequestTimeOffset = DateTimeOffset.Now;

        this.Resolve<IPacketSender>().Send(new BuildingResyncRequest());
        ErrorMessage.AddMessage("Issued a resync request for bases");
        // TODO: Localize
    }

    public void StartResync(Dictionary<Entity, int> entities)
    {
        Resyncing = true;
        FailedOperations = 0;
        BuildQueue.Clear();
        InitializeOperations(entities.ToDictionary(pair => pair.Key.Id, pair => pair.Value));
    }

    public void InitializeOperations(Dictionary<NitroxId, int> operations)
    {
        foreach (KeyValuePair<NitroxId, int> pair in operations)
        {
            Log.Debug($"Resetting {pair.Key} to {pair.Value}");
            EnsureTracker(pair.Key).ResetToId(pair.Value);
        }
    }


    public static Transform GetParentOrGlobalRoot(NitroxId id)
    {
        if (id != null && NitroxEntity.TryGetObjectFrom(id, out GameObject parentObject))
        {
            return parentObject.transform;
        }
        else
        {
            return LargeWorldStreamer.main.globalRoot.transform;
        }
    }

    private void CleanCooldowns()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        List<NitroxId> keysToRemove = BasesCooldown.Where(entry => (now - entry.Value).TotalMilliseconds >= MULTIPLAYER_BUILD_COOLDOWN).Select(e => e.Key).ToList();
        keysToRemove.ForEach(key => BasesCooldown.Remove(key));
    }

    public OperationTracker EnsureTracker(NitroxId baseId)
    {
        if (!Operations.TryGetValue(baseId, out OperationTracker tracker))
        {
            Operations[baseId] = tracker = new(baseId);
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

    public class TemporaryBuildData
    {
        public NitroxId Id;
        public InteriorPieceEntity NewWaterPark;
        public List<NitroxId> MovedChildrenIds;
        public (NitroxId, NitroxId) ChildrenTransfer;
        public bool Transfer;

        public void Fill(PieceDeconstructed pieceDeconstructed)
        {
            Id = pieceDeconstructed.PieceId;
            if (pieceDeconstructed is WaterParkDeconstructed waterParkDeconstructed)
            {
                NewWaterPark = waterParkDeconstructed.NewWaterPark;
                MovedChildrenIds = waterParkDeconstructed.MovedChildrenIds;
                Transfer = waterParkDeconstructed.Transfer;
            }
        }

        public void Reset()
        {
            Id = null;
            NewWaterPark = null;
            MovedChildrenIds = null;
            ChildrenTransfer = (null, null);
            Transfer = false;
        }
    }
}
