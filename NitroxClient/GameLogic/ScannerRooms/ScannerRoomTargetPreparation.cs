using System;
using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Prepares one snapshot's target resources incrementally. Prepared resources remain private until every target has
/// been visited, allowing the owning Scanner Room to replace its vanilla-facing collection atomically.
/// </summary>
internal sealed class ScannerRoomTargetPreparation<TResource> : IDisposable where TResource : class
{
    private readonly IEnumerator<ScannerResourceTarget> targetEnumerator;
    private readonly HashSet<ScannerRoomVirtualResourceKey> includedKeys = [];
    private List<TResource> preparedResources;
    private int remainingTargetCount;
    private bool cancelled;
    private bool enumeratorDisposed;
    private bool resourcesTaken;

    public ScannerRoomSnapshot Snapshot { get; }
    public bool IsComplete { get; private set; }

    public ScannerRoomTargetPreparation(ScannerRoomSnapshot snapshot)
    {
        Snapshot = snapshot;
        targetEnumerator = snapshot.Targets.GetEnumerator();
        remainingTargetCount = snapshot.Targets.Count;
        preparedResources = new List<TResource>(Math.Min(remainingTargetCount, 256));

        if (remainingTargetCount == 0)
        {
            Complete();
        }
    }

    /// <returns>The number of input targets visited. Duplicate or rejected targets still consume budget.</returns>
    public int Advance(int targetBudget, Func<ScannerResourceTarget, TResource?> prepareTarget)
    {
        if (targetBudget < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetBudget));
        }
        if (prepareTarget == null)
        {
            throw new ArgumentNullException(nameof(prepareTarget));
        }
        if (cancelled || IsComplete || targetBudget == 0)
        {
            return 0;
        }

        int visitedTargets = 0;
        while (visitedTargets < targetBudget && remainingTargetCount > 0)
        {
            if (!targetEnumerator.MoveNext())
            {
                Cancel();
                throw new InvalidOperationException("Scanner Room target count changed while preparing a snapshot");
            }

            ScannerResourceTarget target = targetEnumerator.Current;
            remainingTargetCount--;
            visitedTargets++;

            ScannerRoomVirtualResourceKey key = new(target.EntityId, target.TrackerIndex);
            if (!includedKeys.Add(key))
            {
                continue;
            }

            if (prepareTarget(target) is { } preparedResource)
            {
                preparedResources.Add(preparedResource);
            }
        }

        if (remainingTargetCount == 0)
        {
            Complete();
        }
        return visitedTargets;
    }

    public bool TryTakeCompleted(out List<TResource>? resources)
    {
        if (cancelled || !IsComplete || resourcesTaken)
        {
            resources = null;
            return false;
        }

        resourcesTaken = true;
        resources = preparedResources;
        preparedResources = [];
        return true;
    }

    public void Cancel()
    {
        if (cancelled)
        {
            return;
        }

        cancelled = true;
        preparedResources.Clear();
        DisposeEnumerator();
    }

    public void Dispose() => Cancel();

    private void Complete()
    {
        IsComplete = true;
        DisposeEnumerator();
    }

    private void DisposeEnumerator()
    {
        if (enumeratorDisposed)
        {
            return;
        }

        enumeratorDisposed = true;
        targetEnumerator.Dispose();
    }
}
