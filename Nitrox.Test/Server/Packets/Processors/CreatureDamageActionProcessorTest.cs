using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

[TestClass]
public class CreatureDamageActionProcessorTest
{
    private RecordingPacketSender packetSender;
    private SimulationOwnershipData simulationOwnershipData;
    private CreatureDamageActionProcessor processor;
    private Player attacker;
    private Player simulatingPlayer;
    private AuthProcessorContext context;

    [TestInitialize]
    public void Init()
    {
        packetSender = new RecordingPacketSender();
        simulationOwnershipData = new SimulationOwnershipData();
        processor = new CreatureDamageActionProcessor(simulationOwnershipData, NullLogger<CreatureDamageActionProcessor>.Instance);
        attacker = CreatePlayer(1);
        simulatingPlayer = CreatePlayer(2);
        context = new AuthProcessorContext(attacker, packetSender);
    }

    [TestMethod]
    public async Task Process_CreatureSimulatedByAnotherPlayer_SendsHitToSimulatingPlayer()
    {
        NitroxId creatureId = new();
        simulationOwnershipData.TryToAcquire(creatureId, simulatingPlayer, SimulationLockType.TRANSIENT);
        CreatureDamageAction hit = CreateHit(creatureId, 25f, 0);

        await processor.Process(context, hit);

        packetSender.Sent.Should().ContainSingle();
        packetSender.Sent[0].SessionId.Should().Be(simulatingPlayer.SessionId);
        packetSender.Sent[0].Packet.Should().BeSameAs(hit);
    }

    [TestMethod]
    public async Task Process_ExclusiveLockHolder_ReceivesHit()
    {
        NitroxId creatureId = new();
        simulationOwnershipData.TryToAcquire(creatureId, simulatingPlayer, SimulationLockType.EXCLUSIVE);

        await processor.Process(context, CreateHit(creatureId, 25f, 0));

        packetSender.Sent.Should().ContainSingle().Which.SessionId.Should().Be(simulatingPlayer.SessionId);
    }

    [TestMethod]
    public async Task Process_NobodySimulatesCreature_SendsHitBackToSender()
    {
        await processor.Process(context, CreateHit(new NitroxId(), 25f, 0));

        packetSender.Sent.Should().ContainSingle().Which.SessionId.Should().Be(attacker.SessionId);
    }

    [TestMethod]
    public async Task Process_SenderSimulatesCreature_SendsHitBackToSender()
    {
        NitroxId creatureId = new();
        simulationOwnershipData.TryToAcquire(creatureId, attacker, SimulationLockType.TRANSIENT);

        await processor.Process(context, CreateHit(creatureId, 25f, 0));

        packetSender.Sent.Should().ContainSingle().Which.SessionId.Should().Be(attacker.SessionId);
    }

    [TestMethod]
    public async Task Process_ReroutedHitWithSimulatingPlayer_SendsHitToSimulatingPlayer()
    {
        NitroxId creatureId = new();
        simulationOwnershipData.TryToAcquire(creatureId, simulatingPlayer, SimulationLockType.TRANSIENT);

        await processor.Process(context, CreateHit(creatureId, 25f, 1));

        packetSender.Sent.Should().ContainSingle().Which.SessionId.Should().Be(simulatingPlayer.SessionId);
    }

    [TestMethod]
    public async Task Process_ReroutedHitWithoutSimulatingPlayer_DropsHit()
    {
        await processor.Process(context, CreateHit(new NitroxId(), 25f, 1));

        packetSender.Sent.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Process_TooManyHops_DropsHit()
    {
        NitroxId creatureId = new();
        simulationOwnershipData.TryToAcquire(creatureId, simulatingPlayer, SimulationLockType.TRANSIENT);

        await processor.Process(context, CreateHit(creatureId, 25f, CreatureDamageAction.MAX_HOPS + 1));

        packetSender.Sent.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(0f)]
    [DataRow(-25f)]
    [DataRow(float.NaN)]
    [DataRow(float.PositiveInfinity)]
    public async Task Process_InvalidDamage_DropsHit(float damage)
    {
        NitroxId creatureId = new();
        simulationOwnershipData.TryToAcquire(creatureId, simulatingPlayer, SimulationLockType.TRANSIENT);

        await processor.Process(context, CreateHit(creatureId, damage, 0));

        packetSender.Sent.Should().BeEmpty();
    }

    private static CreatureDamageAction CreateHit(NitroxId creatureId, float damage, int hops)
    {
        return new CreatureDamageAction(creatureId, damage, DamageType.Normal, NitroxVector3.Zero, Optional.Empty, (byte)hops);
    }

    private static Player CreatePlayer(ushort sessionId)
    {
        return new Player(sessionId, sessionId, $"Player{sessionId}", false, null, NitroxVector3.Zero, NitroxQuaternion.Identity, new NitroxId(), Optional.Empty, Perms.PLAYER,
                          new PlayerStatsData(100, 100, 100, 100, 100, 0), SubnauticaGameMode.SURVIVAL, [], [], new Dictionary<string, NitroxId>(), new Dictionary<string, float>(),
                          new Dictionary<string, PingInstancePreference>(), [], false, true);
    }

    private sealed class RecordingPacketSender : IPacketSender
    {
        public List<(Packet Packet, SessionId SessionId)> Sent { get; } = [];

        public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet
        {
            Sent.Add((packet, sessionId));
            return ValueTask.CompletedTask;
        }

        public ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet => throw new AssertFailedException("A hit must only reach one player");

        public ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet => throw new AssertFailedException("A hit must only reach one player");
    }
}
