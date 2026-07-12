using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class FireExtinguisherUse(SessionId playerId, bool spraying) : Packet()
{
	public SessionId PlayerId { get; } = playerId;

	public bool Spraying { get; } = spraying;

	public override string ToString()
	{
		return $"[FireExtinguisherUse - PlayerId: {PlayerId}, Spraying: {Spraying}]";
	}
}




