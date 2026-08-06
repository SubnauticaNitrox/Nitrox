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

        ScannerResourceDescriptor descriptor = Parse(mineral, TechType.Titanium).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.Quartz.ToString());
        descriptor.TrackerIndex.Should().Be(0);
    }

    [TestMethod]
    public void FragmentFixtureUsesPrefabTechType()
    {
        ScannerResourcePrefabNode fragment = Node(
            prefabTechType: TechType.SeamothFragment,
            trackers: [Tracker()]);

        ScannerResourceDescriptor descriptor = Parse(fragment, TechType.Titanium).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.SeamothFragment.ToString());
    }

    [TestMethod]
    public void EggFixtureUsesPrefabTechType()
    {
        ScannerResourcePrefabNode egg = Node(
            prefabTechType: TechType.StalkerEgg,
            trackers: [Tracker()]);

        ScannerResourceDescriptor descriptor = Parse(egg, TechType.Titanium).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.StalkerEgg.ToString());
    }

    [TestMethod]
    public void DrillableFixtureUsesOverrideTechType()
    {
        ScannerResourcePrefabNode drillable = Node(
            prefabTechType: TechType.Titanium,
            trackers: [Tracker(techType: TechType.Quartz, overrideTechType: TechType.Copper)]);

        ScannerResourceDescriptor descriptor = Parse(drillable, TechType.Diamond).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.Copper.ToString());
    }

    [DataTestMethod]
    [DataRow(TechType.ScrapMetal)]
    [DataRow(TechType.LimestoneChunk)]
    [DataRow(TechType.SandstoneChunk)]
    [DataRow(TechType.ShaleChunk)]
    public void ClassTechTypeFallbackResolvesTrackedResource(TechType classTechType)
    {
        ScannerResourcePrefabNode resource = Node(trackers: [Tracker()]);

        ScannerResourceDescriptor descriptor = Parse(resource, classTechType).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(classTechType.ToString());
    }

    [TestMethod]
    public void DisabledTrackerFixtureProducesNoDescriptor()
    {
        ScannerResourcePrefabNode disabled = Node(trackers: [Tracker(false, TechType.Quartz)]);

        Parse(disabled, TechType.ScrapMetal).Should().BeEmpty();
    }

    [TestMethod]
    public void ClassTechTypeFallbackWithoutTrackerProducesNoDescriptor()
    {
        Parse(Node(), TechType.ScrapMetal).Should().BeEmpty();
    }

    [TestMethod]
    public void UnresolvedTrackerProducesNoDescriptor()
    {
        Parse(Node(trackers: [Tracker()])).Should().BeEmpty();
    }

    [TestMethod]
    public void InvalidClassTechTypeProducesNoDescriptor()
    {
        ScannerResourcePrefabNode resource = Node(trackers: [Tracker()]);

        ScannerResourcePrefabParser.Parse(resource, int.MaxValue).Should().BeEmpty();
    }

    [TestMethod]
    public void ChildTechTagOverridesClassTechTypeFallback()
    {
        ScannerResourcePrefabNode prefab = Node(
            children: [Node(prefabTechType: TechType.Quartz, trackers: [Tracker()])]);

        ScannerResourceDescriptor descriptor = Parse(prefab, TechType.ScrapMetal).Should().ContainSingle().Which;

        descriptor.TechType.Name.Should().Be(TechType.Quartz.ToString());
    }

    [TestMethod]
    public void ChildFixtureComposesRelativeTransformAndPreservesTrackerOrdinal()
    {
        ScannerResourcePrefabNode prefab = Node(
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

        ScannerResourceDescriptor[] descriptors = Parse(prefab, TechType.Quartz);

        descriptors.Should().HaveCount(2);
        descriptors[0].TrackerIndex.Should().Be(0);
        descriptors[1].TrackerIndex.Should().Be(2);
        descriptors[1].TechType.Name.Should().Be(TechType.Quartz.ToString());
        descriptors[1].RelativePosition.Should().Be(new NitroxVector3(12, 26, 42));
    }

    private static ScannerResourceDescriptor[] Parse(ScannerResourcePrefabNode prefab, TechType classTechType = TechType.None) =>
        ScannerResourcePrefabParser.Parse(prefab, (int)classTechType);

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
