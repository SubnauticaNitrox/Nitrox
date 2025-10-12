using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class BenchChangedProcessor : ClientPacketProcessor<BenchChanged>
{
    private readonly PlayerManager remotePlayerManager;

    public BenchChangedProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(BenchChanged benchChanged)
    {
        if (!remotePlayerManager.TryFind(benchChanged.PlayerId, out RemotePlayer remotePlayer))
        {
            Log.Error($"Couldn't find {nameof(RemotePlayer)} for {benchChanged.PlayerId}");
            return;
        }
        if (!NitroxEntity.TryGetObjectFrom(benchChanged.BenchId, out GameObject bench))
        {
            Log.Error($"Couldn't find GameObject for {benchChanged.BenchId}");
            return;
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
            return;
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
    }
}
