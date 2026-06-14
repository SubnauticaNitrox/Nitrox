using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Bases;

internal static class MapRoomCameraRegistry
{
    private static readonly Dictionary<NitroxId, MapRoomCameraRegistryEntry> cameraEntriesById = [];

    public static MapRoomCameraRegistryEntry RegisterOrUpdateCamera(NitroxId cameraId, NitroxId mapRoomId = null, int dockingIndex = -1)
    {
        if (!cameraEntriesById.TryGetValue(cameraId, out MapRoomCameraRegistryEntry entry))
        {
            entry = new MapRoomCameraRegistryEntry(cameraId, GetLowestAvailableNumber(), mapRoomId, dockingIndex);
            cameraEntriesById[cameraId] = entry;
            return entry;
        }

        if (mapRoomId != null && dockingIndex >= 0)
        {
            entry.MapRoomId = mapRoomId;
            entry.DockingIndex = dockingIndex;
        }

        return entry;
    }

    public static MapRoomCameraRegistryEntry RegisterDockedCamera(NitroxId cameraId, NitroxId mapRoomId, int dockingIndex)
    {
        return RegisterOrUpdateCamera(cameraId, mapRoomId, dockingIndex);
    }

    public static MapRoomCameraRegistryEntry MarkCameraUndocked(NitroxId cameraId)
    {
        MapRoomCameraRegistryEntry entry = RegisterOrUpdateCamera(cameraId);
        entry.MapRoomId = null;
        entry.DockingIndex = -1;
        return entry;
    }

    public static void DeregisterCamera(NitroxId cameraId)
    {
        cameraEntriesById.Remove(cameraId);
    }

    public static List<MapRoomCameraRegistryEntry> GetSaveData()
    {
        return cameraEntriesById.Values
                                .OrderBy(entry => entry.CameraNumber)
                                .Select(entry => new MapRoomCameraRegistryEntry(entry.CameraId, entry.CameraNumber, entry.MapRoomId, entry.DockingIndex))
                                .ToList();
    }

    public static void LoadFromSave(IEnumerable<MapRoomCameraRegistryEntry> savedEntries, EntityRegistry entityRegistry)
    {
        cameraEntriesById.Clear();

        if (savedEntries == null)
        {
            return;
        }

        foreach (MapRoomCameraRegistryEntry savedEntry in savedEntries.OrderBy(entry => entry.CameraNumber))
        {
            if (savedEntry.CameraId == null || savedEntry.CameraNumber <= 0)
            {
                continue;
            }

            // Do not reserve a number for a camera entity that no longer exists in the server save.
            if (!entityRegistry.TryGetEntityById<Entity>(savedEntry.CameraId, out _))
            {
                continue;
            }

            if (cameraEntriesById.ContainsKey(savedEntry.CameraId))
            {
                continue;
            }

            if (cameraEntriesById.Values.Any(entry => entry.CameraNumber == savedEntry.CameraNumber))
            {
                continue;
            }

            cameraEntriesById[savedEntry.CameraId] = new MapRoomCameraRegistryEntry(
                savedEntry.CameraId,
                savedEntry.CameraNumber,
                savedEntry.MapRoomId,
                savedEntry.DockingIndex);
        }
    }

    public static string GetDebugText()
    {
        return string.Join(", ",
            cameraEntriesById.Values
                             .OrderBy(entry => entry.CameraNumber)
                             .Select(entry => $"{entry.CameraNumber}:{entry.CameraId}:mapRoom={entry.MapRoomId}:dock={entry.DockingIndex}"));
    }

    private static int GetLowestAvailableNumber()
    {
        int number = 1;
        HashSet<int> usedNumbers = new(cameraEntriesById.Values
                                                        .Where(entry => entry.CameraNumber > 0)
                                                        .Select(entry => entry.CameraNumber));

        while (usedNumbers.Contains(number))
        {
            number++;
        }

        return number;
    }
}
