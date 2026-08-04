using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Test.Server.Resources;

[TestClass]
public class ScannerResourceDescriptorFactoryTest
{
    [TestMethod]
    public void TryCreate_OverrideTechTypePresent_UsesOverrideCategory()
    {
        bool created = ScannerResourceDescriptorFactory.TryCreate(
            true,
            (int)TechType.Quartz,
            (int)TechType.Copper,
            (int)TechType.Titanium,
            3,
            new NitroxVector3(1, 2, 3),
            out ScannerResourceDescriptor descriptor);

        created.Should().BeTrue();
        descriptor.TechType.Name.Should().Be(TechType.Copper.ToString());
        descriptor.TrackerIndex.Should().Be(3);
        descriptor.RelativePosition.Should().Be(new NitroxVector3(1, 2, 3));
    }

    [TestMethod]
    public void TryCreate_NoOverride_UsesSerializedTrackerTechType()
    {
        bool created = ScannerResourceDescriptorFactory.TryCreate(
            true,
            (int)TechType.Quartz,
            (int)TechType.None,
            (int)TechType.Titanium,
            0,
            NitroxVector3.Zero,
            out ScannerResourceDescriptor descriptor);

        created.Should().BeTrue();
        descriptor.TechType.Name.Should().Be(TechType.Quartz.ToString());
    }

    [TestMethod]
    public void TryCreate_NoSerializedTypes_UsesPrefabTechType()
    {
        bool created = ScannerResourceDescriptorFactory.TryCreate(
            true,
            (int)TechType.None,
            (int)TechType.None,
            (int)TechType.Titanium,
            0,
            NitroxVector3.Zero,
            out ScannerResourceDescriptor descriptor);

        created.Should().BeTrue();
        descriptor.TechType.Name.Should().Be(TechType.Titanium.ToString());
    }

    [TestMethod]
    public void TryCreate_DisabledTracker_ReturnsFalse()
    {
        bool created = ScannerResourceDescriptorFactory.TryCreate(
            false,
            (int)TechType.Quartz,
            (int)TechType.None,
            (int)TechType.None,
            0,
            NitroxVector3.Zero,
            out _);

        created.Should().BeFalse();
    }

    [TestMethod]
    public void TryCreate_NoResolvableTechType_ReturnsFalse()
    {
        bool created = ScannerResourceDescriptorFactory.TryCreate(
            true,
            (int)TechType.None,
            (int)TechType.None,
            (int)TechType.None,
            0,
            NitroxVector3.Zero,
            out _);

        created.Should().BeFalse();
    }
}
