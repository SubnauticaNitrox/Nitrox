using System.Threading.Tasks;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FireExtinguisherUseProcessor : IAuthPacketProcessor<FireExtinguisherUse>, IAuthPacketProcessor, IPacketProcessor, IPacketProcessor<AuthProcessorContext, FireExtinguisherUse>
{
	public async Task Process(AuthProcessorContext context, FireExtinguisherUse packet)
	{
		await context.SendToOthersAsync<FireExtinguisherUse>(packet);
	}
}


