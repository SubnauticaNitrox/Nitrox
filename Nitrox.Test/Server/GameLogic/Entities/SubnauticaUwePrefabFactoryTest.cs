using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NSubstitute;
using LootDictionary = System.Collections.Generic.Dictionary<string, LootDistributionData.SrcData>;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

[TestClass]
public class SubnauticaUwePrefabFactoryTest
{
    private static LootDistributionData CreateEmptyInitializedLootDistributionData()
    {
        LootDictionary empty = [];
        LootDistributionData data = new();
        data.Initialize(empty);
        return data;
    }

    [TestMethod]
    public async Task TryGetPossiblePrefabsAsync_NullBiome_ReturnsEmptyWithoutCallingResource()
    {
        IEntityDistributionsAccessor accessor = Substitute.For<IEntityDistributionsAccessor>();
        SubnauticaUwePrefabFactory factory = new(accessor);

        List<UwePrefab> result = await factory.TryGetPossiblePrefabsAsync(null);

        result.Should().BeEmpty();
        await accessor.DidNotReceive().GetLootDistributionDataAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task TryGetPossiblePrefabsAsync_ConcurrentCallsSameBiome_LoadsDistributionOnce()
    {
        int callCount = 0;
        IEntityDistributionsAccessor accessor = Substitute.For<IEntityDistributionsAccessor>();
        accessor.GetLootDistributionDataAsync(Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    Interlocked.Increment(ref callCount);
                    return Task.FromResult(CreateEmptyInitializedLootDistributionData());
                });

        SubnauticaUwePrefabFactory factory = new(accessor);

        string biomeName = Enum.GetNames(typeof(BiomeType))[0];
        Task[] tasks = Enumerable.Range(0, 10).Select(_ => factory.TryGetPossiblePrefabsAsync(biomeName)).ToArray();
        await Task.WhenAll(tasks);

        callCount.Should().Be(1);
    }

    [TestMethod]
    public async Task TryGetPossiblePrefabsAsync_SecondCall_ReusesCachedTask()
    {
        IEntityDistributionsAccessor accessor = Substitute.For<IEntityDistributionsAccessor>();
        accessor.GetLootDistributionDataAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(CreateEmptyInitializedLootDistributionData()));

        SubnauticaUwePrefabFactory factory = new(accessor);

        string biomeName = Enum.GetNames(typeof(BiomeType))[0];
        List<UwePrefab> first = await factory.TryGetPossiblePrefabsAsync(biomeName);
        List<UwePrefab> second = await factory.TryGetPossiblePrefabsAsync(biomeName);

        second.Should().BeSameAs(first);
        await accessor.Received(1).GetLootDistributionDataAsync(Arg.Any<CancellationToken>());
    }
}
