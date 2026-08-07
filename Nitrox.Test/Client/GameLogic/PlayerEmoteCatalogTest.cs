using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace Nitrox.Test.Client.GameLogic;

[TestClass]
public sealed class PlayerEmoteCatalogTest
{
    [TestMethod]
    public void ApprovedGroupsCoverEverySoundExactlyOnce()
    {
        PlayerEmoteCatalog.OrderedDefinitions.Select(definition => definition.Group)
                          .Should()
                          .Equal(
                              PlayerEmoteGroup.Yes,
                              PlayerEmoteGroup.Affirmative,
                              PlayerEmoteGroup.Thanks,
                              PlayerEmoteGroup.TeamUp,
                              PlayerEmoteGroup.LetsGo,
                              PlayerEmoteGroup.ShowOff,
                              PlayerEmoteGroup.Attention,
                              PlayerEmoteGroup.SorryOrCeasefire,
                              PlayerEmoteGroup.No);

        PlayerEmoteCatalog.Get(PlayerEmoteGroup.Yes).SoundIndices.Should().Equal(new byte[] { 20, 21, 22, 23 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.Affirmative).SoundIndices.Should().Equal(new byte[] { 12, 17 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.No).SoundIndices.Should().Equal(new byte[] { 10, 11 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.Thanks).SoundIndices.Should().Equal(new byte[] { 0, 13 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.SorryOrCeasefire).SoundIndices.Should().Equal(new byte[] { 2, 4, 5 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.Attention).SoundIndices.Should().Equal(new byte[] { 3, 6 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.TeamUp).SoundIndices.Should().Equal(new byte[] { 14, 15, 16 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.ShowOff).SoundIndices.Should().Equal(new byte[] { 1, 7, 18 });
        PlayerEmoteCatalog.Get(PlayerEmoteGroup.LetsGo).SoundIndices.Should().Equal(new byte[] { 8, 9, 19 });

        PlayerEmoteCatalog.OrderedDefinitions
                          .SelectMany(definition => definition.SoundIndices)
                          .OrderBy(index => index)
                          .Should()
                          .Equal(Enumerable.Range(0, PlayerYell.SOUND_COUNT).Select(index => (byte)index));
    }

    [TestMethod]
    public void PlayerYellsStartsWithYesAndInvalidPlaybackDoesNotChangeIt()
    {
        PlayerYells playerYells = new(null!, null!, null!, null!, null!);

        playerYells.RecentGroup.Should().Be(PlayerEmoteGroup.Yes);
        playerYells.TryYell(PlayerEmoteGroup.Attention).Should().BeFalse();
        playerYells.RecentGroup.Should().Be(PlayerEmoteGroup.Yes);
    }
}
