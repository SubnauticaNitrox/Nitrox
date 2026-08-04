using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Test.Server.Resources;

[TestClass]
public sealed class ScannerResourcePrefabParserTest
{
    [TestMethod]
    public void MineralFixtureUsesTrackerTechType()
    {
        ScannerResourcePrefabNode mineral = Node(trackers: [Tracker(techType: TechType.Quartz)]);

        ScannerResourceDescriptor descriptor = ScannerResourcePrefabParser.Parse(mineral).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.Quartz.ToString());
        descriptor.TrackerIndex.Should().Be(0);
    }

    [TestMethod]
    public void FragmentFixtureUsesPrefabTechType()
    {
        ScannerResourcePrefabNode fragment = Node(
            prefabTechType: TechType.SeamothFragment,
            trackers: [Tracker()]);

        ScannerResourceDescriptor descriptor = ScannerResourcePrefabParser.Parse(fragment).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.SeamothFragment.ToString());
    }

    [TestMethod]
    public void EggFixtureUsesPrefabTechType()
    {
        ScannerResourcePrefabNode egg = Node(
            prefabTechType: TechType.StalkerEgg,
            trackers: [Tracker()]);

        ScannerResourceDescriptor descriptor = ScannerResourcePrefabParser.Parse(egg).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.StalkerEgg.ToString());
    }

    [TestMethod]
    public void DrillableFixtureUsesOverrideTechType()
    {
        ScannerResourcePrefabNode drillable = Node(
            prefabTechType: TechType.Titanium,
            trackers: [Tracker(techType: TechType.Quartz, overrideTechType: TechType.Copper)]);

        ScannerResourceDescriptor descriptor = ScannerResourcePrefabParser.Parse(drillable).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.Copper.ToString());
    }

    [TestMethod]
    public void DisabledTrackerFixtureProducesNoDescriptor()
    {
        ScannerResourcePrefabNode disabled = Node(trackers: [Tracker(false, TechType.Quartz)]);

        ScannerResourcePrefabParser.Parse(disabled).Should().BeEmpty();
    }

    [TestMethod]
    public void ChildFixtureComposesRelativeTransformAndPreservesTrackerOrdinal()
    {
        ScannerResourcePrefabNode prefab = Node(
            prefabTechType: TechType.Quartz,
            trackers: [Tracker()],
            children:
            [
                Node(
                    localPosition: new NitroxVector3(10, 20, 30),
                    localScale: new NitroxVector3(2, 3, 4),
                    trackers: [Tracker(false)],
                    children:
                    [
                        Node(
                            localPosition: new NitroxVector3(1, 2, 3),
                            trackers: [Tracker()])
                    ])
            ]);

        ScannerResourceDescriptor[] descriptors = ScannerResourcePrefabParser.Parse(prefab);

        descriptors.Should().HaveCount(2);
        descriptors[0].TrackerIndex.Should().Be(0);
        descriptors[1].TrackerIndex.Should().Be(2);
        descriptors[1].TechType.Name.Should().Be(TechType.Quartz.ToString());
        descriptors[1].RelativePosition.Should().Be(new NitroxVector3(12, 26, 42));
    }

    private static ScannerResourcePrefabNode Node(
        TechType prefabTechType = TechType.None,
        IReadOnlyList<ScannerResourceTrackerData>? trackers = null,
        IReadOnlyList<ScannerResourcePrefabNode>? children = null,
        NitroxVector3? localPosition = null,
        NitroxQuaternion? localRotation = null,
        NitroxVector3? localScale = null) =>
        new(
            localPosition ?? NitroxVector3.Zero,
            localRotation ?? NitroxQuaternion.Identity,
            localScale ?? NitroxVector3.One,
            (int)prefabTechType,
            trackers ?? [],
            children ?? []);

    private static ScannerResourceTrackerData Tracker(
        bool enabled = true,
        TechType techType = TechType.None,
        TechType overrideTechType = TechType.None) =>
        new(enabled, (int)techType, (int)overrideTechType);
}
