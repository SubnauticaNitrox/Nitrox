using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace Nitrox.Test.Model.DataStructures;

[TestClass]
public sealed class ScannerRoomScanStateTest
{
    [TestMethod]
    public void NoneSelectionIsCanonicalizedToNullWithoutChangingVersion()
    {
        ScannerRoomScanState state = new(NitroxTechType.None, 17);

        state.SelectedTechType.Should().BeNull();
        state.Version.Should().Be(17);
    }

    [TestMethod]
    public void EmptyStateHasNoSelectionAtVersionZero()
    {
        ScannerRoomScanState.Empty.SelectedTechType.Should().BeNull();
        ScannerRoomScanState.Empty.Version.Should().Be(0);
    }
}
