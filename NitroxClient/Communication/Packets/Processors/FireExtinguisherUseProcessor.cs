using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireExtinguisherUseProcessor(PlayerManager playerManager) : IClientPacketProcessor<FireExtinguisherUse>, IClientPacketProcessor, IPacketProcessor, IPacketProcessor<ClientProcessorContext, FireExtinguisherUse>
{
	private readonly PlayerManager playerManager = playerManager;

	public Task Process(ClientProcessorContext context, FireExtinguisherUse packet)
	{
		if (!playerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer) || !remotePlayer.ItemAttachPoint)
		{
			return Task.CompletedTask;
		}
		FireExtinguisher componentInChildren = remotePlayer.ItemAttachPoint.GetComponentInChildren<FireExtinguisher>(includeInactive: true);
		if (!componentInChildren)
		{
			return Task.CompletedTask;
		}
		if ((bool)componentInChildren.fxControl)
		{
			if (packet.Spraying)
			{
				componentInChildren.fxControl.Play(0);
			}
			else
			{
				componentInChildren.fxControl.Stop(0);
			}
		}
		if ((bool)componentInChildren.soundEmitter)
		{
			if (packet.Spraying)
			{
				componentInChildren.soundEmitter.Play();
			}
			else
			{
				componentInChildren.soundEmitter.Stop();
			}
		}
		return Task.CompletedTask;
	}
}


