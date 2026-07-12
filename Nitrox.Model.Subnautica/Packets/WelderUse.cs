using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class WelderUse(SessionId playerId, bool welding) : Packet()
{
	public SessionId PlayerId { get; } = playerId;

	public bool Welding { get; } = welding;

	public override string ToString()
	{
		return $"[WelderUse - PlayerId: {PlayerId}, Welding: {Welding}]";
	}
}




