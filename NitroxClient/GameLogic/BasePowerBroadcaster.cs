using Nitrox.Model.DataStructures;
using NitroxClient.Extensions;
using UnityEngine;

namespace NitroxClient.GameLogic;

public static class BasePowerBroadcaster
{
	private const float BROADCAST_THROTTLE_SECONDS = 1f;

	public static void BroadcastIfOwner(Component owner, PowerSource powerSource, SimulationOwnership simulationOwnership, Entities entities)
	{
		if ((bool)powerSource && owner.TryGetNitroxId(out NitroxId nitroxId) && simulationOwnership.HasAnyLockType(nitroxId))
		{
			entities.EntityMetadataChangedThrottled(powerSource, nitroxId, 1f);
		}
	}
}


