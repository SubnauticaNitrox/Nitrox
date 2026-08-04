using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Holds Scanner Room snapshot pages until the Unity update loop can process them within a frame budget.
/// </summary>
internal sealed class ScannerRoomSnapshotPageQueue
{
    private readonly object queueLock = new();
    private readonly Queue<ScannerRoomSnapshotPage> pages = [];
    private readonly Dictionary<(NitroxId MapRoomId, uint RequestId), int> queuedRequestCounts = [];

    public int Count
    {
        get
        {
            lock (queueLock)
            {
                return pages.Count;
            }
        }
    }

    public void Enqueue(ScannerRoomSnapshotPage page)
    {
        lock (queueLock)
        {
            pages.Enqueue(page);
            IncrementRequestCount(page);
        }
    }

    public bool HasQueuedPage(NitroxId mapRoomId, uint requestId)
    {
        lock (queueLock)
        {
            return queuedRequestCounts.ContainsKey((mapRoomId, requestId));
        }
    }

    public int Process(int pageBudget, Action<ScannerRoomSnapshotPage> processPage)
    {
        if (pageBudget < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageBudget));
        }

        int processed = 0;
        while (processed < pageBudget)
        {
            ScannerRoomSnapshotPage page;
            lock (queueLock)
            {
                if (pages.Count == 0)
                {
                    break;
                }
                page = pages.Dequeue();
                DecrementRequestCount(page);
            }

            processPage(page);
            processed++;
        }
        return processed;
    }

    public int RemoveRoom(NitroxId mapRoomId)
    {
        lock (queueLock)
        {
            int initialCount = pages.Count;
            int removed = 0;
            for (int i = 0; i < initialCount; i++)
            {
                ScannerRoomSnapshotPage page = pages.Dequeue();
                if (page.MapRoomId.Equals(mapRoomId))
                {
                    DecrementRequestCount(page);
                    removed++;
                }
                else
                {
                    pages.Enqueue(page);
                }
            }
            return removed;
        }
    }

    public void Clear()
    {
        lock (queueLock)
        {
            pages.Clear();
            queuedRequestCounts.Clear();
        }
    }

    private void IncrementRequestCount(ScannerRoomSnapshotPage page)
    {
        (NitroxId MapRoomId, uint RequestId) key = (page.MapRoomId, page.RequestId);
        queuedRequestCounts.TryGetValue(key, out int count);
        queuedRequestCounts[key] = count + 1;
    }

    private void DecrementRequestCount(ScannerRoomSnapshotPage page)
    {
        (NitroxId MapRoomId, uint RequestId) key = (page.MapRoomId, page.RequestId);
        int count = queuedRequestCounts[key];
        if (count == 1)
        {
            queuedRequestCounts.Remove(key);
        }
        else
        {
            queuedRequestCounts[key] = count - 1;
        }
    }
}
