using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class BedExitAnimation : Packet
{
	public SessionId SessionId { get; }

	public NitroxId BedId { get; }

	public string AnimationKey { get; }

	public BedExitAnimation(SessionId sessionId, NitroxId bedId, string animationKey)
	{
		SessionId = sessionId;
		BedId = bedId;
		AnimationKey = animationKey;
	}
}




