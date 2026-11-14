using System;
using Nitrox.Model.Packets;
using static Nitrox.Model.Subnautica.Packets.EntityTransformUpdates;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class LastEntityTransformUpdate(EntityTransformUpdate lastUpdate) : Packet
{
    public EntityTransformUpdate LastUpdate { get; set; } = lastUpdate;
}
