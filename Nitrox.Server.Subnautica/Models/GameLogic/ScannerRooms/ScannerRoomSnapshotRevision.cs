using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal static class ScannerRoomSnapshotRevision
{
    private const ulong OFFSET_BASIS = 14695981039346656037UL;
    private const ulong PRIME = 1099511628211UL;

    public static ulong Compute(
        float effectiveRange,
        NitroxTechType? selectedTechType,
        IReadOnlyList<ScannerResourceSummary> summaries,
        IReadOnlyList<ScannerResourceTarget> targets)
    {
        ulong hash = OFFSET_BASIS;
        Add(ref hash, unchecked((uint)BitConverter.SingleToInt32Bits(effectiveRange)));
        Add(ref hash, selectedTechType?.Name);

        foreach (ScannerResourceSummary summary in summaries)
        {
            Add(ref hash, summary.TechType.Name);
            Add(ref hash, unchecked((uint)summary.Count));
        }
        foreach (ScannerResourceTarget target in targets)
        {
            Add(ref hash, target.EntityId.ToString());
            Add(ref hash, target.TrackerIndex);
            Add(ref hash, target.TechType.Name);
            Add(ref hash, unchecked((uint)BitConverter.SingleToInt32Bits(target.Position.X)));
            Add(ref hash, unchecked((uint)BitConverter.SingleToInt32Bits(target.Position.Y)));
            Add(ref hash, unchecked((uint)BitConverter.SingleToInt32Bits(target.Position.Z)));
        }

        return hash == 0 ? 1 : hash;
    }

    private static void Add(ref ulong hash, string? value)
    {
        if (value == null)
        {
            Add(ref hash, uint.MaxValue);
            return;
        }
        foreach (char character in value)
        {
            Add(ref hash, character);
        }
        Add(ref hash, 0);
    }

    private static void Add(ref ulong hash, uint value)
    {
        for (int shift = 0; shift < 32; shift += 8)
        {
            hash ^= (byte)(value >> shift);
            hash *= PRIME;
        }
    }
}
