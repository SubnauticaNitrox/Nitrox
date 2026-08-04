using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Helper;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

[TestClass]
public class ScannerBatchCoverageTest
{
    [TestMethod]
    public void ZeroRadiusReturnsContainingBatch()
    {
        NitroxVector3 center = new(0, 0, 0);

        IReadOnlyList<NitroxInt3> result = ScannerBatchCoverage.EnumerateIntersectingBatches(center, 0);

        result.Should().ContainSingle();
        result[0].Should().Be(new AbsoluteEntityCell(center, 0).BatchId);
    }

    [TestMethod]
    public void ExactSphereBoundaryIncludesAdjacentBatch()
    {
        NitroxInt3 batch = new(1, 1, 1);
        NitroxInt3 minimum = batch * SubnauticaMap.BatchSize - SubnauticaMap.BatchDimensionCenter;
        NitroxVector3 center = new(minimum.X - 10, minimum.Y + 80, minimum.Z + 80);

        IReadOnlyList<NitroxInt3> result = ScannerBatchCoverage.EnumerateIntersectingBatches(center, 10);

        result.Should().Contain(batch);
    }

    [TestMethod]
    public void BatchesAreClampedToMapBounds()
    {
        NitroxInt3 mapOffset = SubnauticaMap.BatchDimensionCenter;
        NitroxVector3 mapCorner = new(-mapOffset.X, -mapOffset.Y, -mapOffset.Z);

        IReadOnlyList<NitroxInt3> result = ScannerBatchCoverage.EnumerateIntersectingBatches(mapCorner, 500);

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(batch => batch.X >= 0 && batch.Y >= 0 && batch.Z >= 0 &&
                                             batch.X < SubnauticaMap.DimensionsInBatches.X &&
                                             batch.Y < SubnauticaMap.DimensionsInBatches.Y &&
                                             batch.Z < SubnauticaMap.DimensionsInBatches.Z);
    }

    [TestMethod]
    public void NegativeRadiusIsRejected()
    {
        Action action = () => ScannerBatchCoverage.EnumerateIntersectingBatches(NitroxVector3.Zero, -1);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
