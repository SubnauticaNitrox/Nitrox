using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.BedSync;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class BedExitAnimationProcessor(PlayerManager playerManager) : IClientPacketProcessor<BedExitAnimation>, IClientPacketProcessor, IPacketProcessor, IPacketProcessor<ClientProcessorContext, BedExitAnimation>
{
	private readonly PlayerManager playerManager = playerManager;

	public async Task Process(ClientProcessorContext context, BedExitAnimation packet)
	{
		if (NitroxEntity.TryGetObjectFrom(packet.BedId, out GameObject gameObject) && gameObject.TryGetComponent<RemoteBedController>(out var bedController) && playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
		{
			bedController.EndBedAnimation(remotePlayer, packet.AnimationKey);
			await Task.Yield();
			bedController.StartBedAnimation(remotePlayer, packet.AnimationKey);
		}
	}
}


