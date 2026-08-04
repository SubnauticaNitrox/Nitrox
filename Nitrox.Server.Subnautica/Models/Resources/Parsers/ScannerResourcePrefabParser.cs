using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal readonly record struct ScannerResourceTrackerData(bool Enabled, int TechType, int OverrideTechType);

internal sealed record ScannerResourcePrefabNode(
    NitroxVector3 LocalPosition,
    NitroxQuaternion LocalRotation,
    NitroxVector3 LocalScale,
    int PrefabTechType,
    IReadOnlyList<ScannerResourceTrackerData> Trackers,
    IReadOnlyList<ScannerResourcePrefabNode> Children);

internal static class ScannerResourcePrefabParser
{
    public static ScannerResourceDescriptor[] Parse(ScannerResourcePrefabNode root)
    {
        List<ScannerResourceDescriptor> descriptors = [];
        int trackerOrdinal = 0;
        ParseNode(root, null, (int)TechType.None, descriptors, ref trackerOrdinal);
        return [.. descriptors];
    }

    private static void ParseNode(
        ScannerResourcePrefabNode node,
        NitroxTransform? parentTransform,
        int inheritedTechType,
        List<ScannerResourceDescriptor> descriptors,
        ref int trackerOrdinal)
    {
        NitroxTransform relativeTransform = new(node.LocalPosition, node.LocalRotation, node.LocalScale)
        {
            Parent = parentTransform
        };
        int prefabTechType = node.PrefabTechType != (int)TechType.None ? node.PrefabTechType : inheritedTechType;

        foreach (ScannerResourceTrackerData tracker in node.Trackers)
        {
            ushort trackerIndex = checked((ushort)trackerOrdinal++);
            if (ScannerResourceDescriptorFactory.TryCreate(
                    tracker.Enabled,
                    tracker.TechType,
                    tracker.OverrideTechType,
                    prefabTechType,
                    trackerIndex,
                    relativeTransform.Position,
                    out ScannerResourceDescriptor descriptor))
            {
                descriptors.Add(descriptor);
            }
        }

        foreach (ScannerResourcePrefabNode child in node.Children)
        {
            ParseNode(child, relativeTransform, prefabTechType, descriptors, ref trackerOrdinal);
        }
    }
}
