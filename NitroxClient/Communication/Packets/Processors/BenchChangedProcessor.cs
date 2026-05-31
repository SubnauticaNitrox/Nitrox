using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BenchChangedProcessor(PlayerManager remotePlayerManager) : IClientPacketProcessor<BenchChanged>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    public Task Process(ClientProcessorContext context, BenchChanged benchChanged)
    {
        if (!remotePlayerManager.TryFind(benchChanged.SessionId, out RemotePlayer remotePlayer))
        {
            Log.Error($"Couldn't find {nameof(RemotePlayer)} for {benchChanged.SessionId}");
            return Task.CompletedTask;
        }
        if (!NitroxEntity.TryGetObjectFrom(benchChanged.BenchId, out GameObject bench))
        {
            Log.Error($"Couldn't find GameObject for {benchChanged.BenchId}");
            return Task.CompletedTask;
        }

        remotePlayer.AnimationController["cinematics_enabled"] = benchChanged.ChangeState != BenchChanged.BenchChangeState.UNSET;
        remotePlayer.AnimationController["bench_sit"] = benchChanged.ChangeState == BenchChanged.BenchChangeState.SITTING_DOWN;
        remotePlayer.AnimationController["bench_stand_up"] = benchChanged.ChangeState == BenchChanged.BenchChangeState.STANDING_UP;

        RemotePlayerBenchBlocker benchBlocker;
        if (bench.GetComponent<Constructable>())
        {
            benchBlocker = bench.EnsureComponent<RemotePlayerBenchBlocker>();
        }
        else if (bench.transform.parent.GetComponent<Constructable>()) // For MultiplayerBenches
        {
            benchBlocker = bench.transform.parent.gameObject.EnsureComponent<RemotePlayerBenchBlocker>();
        }
        else
        {
            Log.Error($"Couldn't find Constructable component on {benchChanged.BenchId} or its parent");
            return Task.CompletedTask;
        }

        switch (benchChanged.ChangeState)
        {
            case BenchChanged.BenchChangeState.SITTING_DOWN:
                benchBlocker.AddPlayerToBench(remotePlayer);
                break;
            case BenchChanged.BenchChangeState.UNSET:
                benchBlocker.RemovePlayerFromBench(remotePlayer);
                break;
        }
        return Task.CompletedTask;
    }
}
